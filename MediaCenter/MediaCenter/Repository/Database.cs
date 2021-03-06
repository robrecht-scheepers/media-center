﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.Sessions.Filters;
using Newtonsoft.Json;

namespace MediaCenter.Repository
{
    public class Database
    {
        private const string BaseConnectionString = "Data Source={0}; Version=3";
        private readonly string _connectionString;

        public Database(string dbPath)
        {
            _connectionString = string.Format(BaseConnectionString, dbPath);
            if(!File.Exists(dbPath))
                CreateNewDatabase(dbPath);
        }

        private SQLiteConnection GetConnection() => new SQLiteConnection(_connectionString);
        private SQLiteCommand GetCommand(SQLiteConnection conn) => new SQLiteCommand(conn);
        private SQLiteCommand GetCommand(SQLiteConnection conn, string cmdTxt) => new SQLiteCommand(conn){CommandText = cmdTxt};

        private void CreateNewDatabase(string dbPath)
        {
            // no need to make this method async as it is executed in the constructor before the window is visible
            string createDbSql;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MediaCenter.Repository.Sql.CreateDB.sql"))
            {
                if (stream == null)
                    throw new Exception("Embedded resource for create DB script not found");
                using (var reader = new StreamReader(stream))
                {
                    createDbSql = reader.ReadToEnd();
                }
            }
            if (string.IsNullOrEmpty(createDbSql))
                throw new Exception("Created DB script is empty");

            if (!Directory.Exists(Path.GetDirectoryName(dbPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            SQLiteConnection.CreateFile(dbPath);
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, createDbSql))
            {
                conn.Open();
                command.ExecuteNonQuery(CommandBehavior.CloseConnection);
            }
        }

        public async Task AddMediaItem(MediaItem item)
        {
            const string cmdTxt = 
                "INSERT INTO MediaInfo(Name, Type, Filename, DateTaken, DateAdded, Favorite, Private, Rotation, Tags, Crop) " +
                "VALUES(@name, @type, @filename, @dateTaken, @dateAdded, @favorite, @private, @rotation, @tags, @crop);";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                LoadCommandParameters(command, item);
                conn.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateMediaItem(MediaItem item)
        {
            const string cmdTxt =
                "UPDATE MediaInfo " +
                "SET Name = @name, Type = @type, Filename = @filename, DateTaken = @dateTaken, DateAdded = @dateAdded, Favorite = @favorite, Private = @private, Rotation = @rotation, Tags = @tags, Crop = @crop " +
                "WHERE Id = @Id;";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                LoadCommandParameters(command, item);
                conn.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteMediaItem(MediaItem item)
        {
            const string cmdTxt =
                "DELETE FROM MediaInfo WHERE Name = @name;";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                command.Parameters.AddWithValue("@name", item.Name);
                conn.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        private void LoadCommandParameters(SQLiteCommand command, MediaItem item)
        {
            command.Parameters.AddWithValue("@id", item.Id);
            command.Parameters.AddWithValue("@name", item.Name);
            command.Parameters.AddWithValue("@type", item.MediaType);
            command.Parameters.AddWithValue("@filename", item.ContentFileName);
            command.Parameters.AddWithValue("@dateTaken", item.DateTaken);
            command.Parameters.AddWithValue("@dateAdded", item.DateAdded);
            command.Parameters.AddWithValue("@favorite", item.Favorite);
            command.Parameters.AddWithValue("@private", item.Private);
            command.Parameters.AddWithValue("@rotation", item.Rotation);
            command.Parameters.AddWithValue("@tags", AggregateTags(item.Tags.ToList()));
            command.Parameters.AddWithValue("@crop",item.Crop == null ? null : JsonConvert.SerializeObject(item.Crop));
        }

        private string AggregateTags(List<string> tags)
        {
            if (tags == null || !tags.Any())
                return "";

            return "#" + tags.Aggregate((a, n) => a + "#" + n) + "#";
        }

        private List<string> SeparateTags(string aggregatedTags)
        {
            return aggregatedTags.Split('#').Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public async Task<bool> IsFavorite(string name)
        {
            var cmdTxt = $"SELECT Favorite FROM MediaInfo WHERE Name = @name";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                command.Parameters.AddWithValue("@name", name);
                conn.Open();
                var result = await command.ExecuteScalarAsync();
                return (long) result > 0;
            }
        }
        
        public async Task<bool> ItemExists(string name)
        {
            var cmdTxt = $"SELECT Id FROM MediaInfo WHERE Name = @name";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                command.Parameters.AddWithValue("@name", name);
                conn.Open();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    return reader.HasRows;
                }
            }
        }

        public async Task<List<string>> GetNameClashes(string name)
        {
            var result = new List<string>();
            var cmdTxt = $"SELECT Name FROM MediaInfo WHERE Name LIKE '{name}%'";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                conn.Open();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                }
            }

            return result;
        }

        public async Task<List<MediaItem>> GetFilteredItemList(IEnumerable<Filter> filters)
        {
            const string baseCommandText =
                "SELECT Id, Name, Type, Filename, DateTaken, DateAdded, Favorite, Private, Rotation, Tags, Crop " +
                "FROM MediaInfo WHERE 1 = 1 {0};";

            using (var command = new SQLiteCommand())
            {
                var conditions = new StringBuilder();
                foreach (var filter in filters)
                {
                    TranslateFilter(filter, conditions, command);
                }
                command.CommandText = string.Format(baseCommandText, conditions);
                return await GetItems(command);
            }
        }

        public async Task<MediaItem> GetItemByName(string name)
        {
            const string commandText =
                "SELECT Id, Name, Type, Filename, DateTaken, DateAdded, Favorite, Private, Rotation, Tags, Crop " +
                "FROM MediaInfo WHERE Name = @name;";

            using (var command = new SQLiteCommand(commandText))
            {
                command.Parameters.AddWithValue("@name", name);
                return (await GetItems(command)).FirstOrDefault();
            }
        }

        private async Task<List<MediaItem>> GetItems(SQLiteCommand queryCommand)
        {
            var results = new List<MediaItem>();

            using (var conn = GetConnection())
            {
                queryCommand.Connection = conn;
                conn.Open();
                using (var reader = await queryCommand.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return new List<MediaItem>();
                    while (reader.Read())
                    {
                        results.Add(new MediaItem(reader.GetString(1), (MediaType)reader.GetInt32(2))
                        {
                            Id = reader.GetInt32(0),
                            ContentFileName = reader.GetString(3),
                            DateTaken = reader.GetDateTime(4),
                            DateAdded = reader.GetDateTime(5),
                            Favorite = reader.GetBoolean(6),
                            Private = reader.GetBoolean(7),
                            Rotation = reader.GetInt32(8),
                            Tags = new ObservableCollection<string>(SeparateTags(reader.GetString(9))),
                            Crop = JsonConvert.DeserializeObject<Crop>(reader.GetStringNullSafe(10, "null"))
                        });
                    }
                }
            }

            return results;
        }



        public async Task<int> GetFilteredItemCount(IEnumerable<Filter> filters)
        {
            const string baseCommandText = "SELECT COUNT(Id) FROM MediaInfo WHERE 1 = 1 {0};";

            using (var conn = GetConnection())
            using (var command = GetCommand(conn))
            {
                var conditions = new StringBuilder();
                foreach (var filter in filters)
                {
                    TranslateFilter(filter, conditions, command);
                }

                command.CommandText = string.Format(baseCommandText, conditions);
                conn.Open();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return 0;
                    reader.Read();
                    return reader.GetInt32(0);
                }
            }
        }

        private void TranslateFilter(Filter filter, StringBuilder conditions, SQLiteCommand command)
        {
            if (filter is DateTakenFilter dateTakenFilter)
            {
                conditions.Append(!filter.Invert
                    ? " AND DateTaken >= @dateTakenFrom AND DateTaken <= @dateTakenTo"
                    : " AND (DateTaken < @dateTakenFrom OR DateTaken > @dateTakenTo)");
                command.Parameters.AddWithValue("@dateTakenFrom", dateTakenFilter.From ?? DateTime.MinValue);
                command.Parameters.AddWithValue("@dateTakenTo", dateTakenFilter.Until?.Date.AddDays(1).AddMilliseconds(-1) ?? DateTime.MaxValue);
            }
            else if (filter is DateAddedFilter dateAddedFilter)
            {
                conditions.Append(!filter.Invert
                    ? " AND DateAdded >= @dateAddedFrom AND DateAdded <= @dateAddedTo"
                    : " AND (DateAdded < @dateAddedFrom OR DateAdded > @dateAddedTo)");
                command.Parameters.AddWithValue("@dateAddedFrom", dateAddedFilter.From ?? DateTime.MinValue);
                command.Parameters.AddWithValue("@dateAddedTo", dateAddedFilter.Until?.Date.AddDays(1).AddMilliseconds(-1) ?? DateTime.MaxValue);
            }
            else if (filter is FavoriteFilter favoriteFilter)
            {
                if (!favoriteFilter.Invert && favoriteFilter.FavoriteSetting == FavoriteFilter.FavoriteOption.OnlyFavorite
                    || favoriteFilter.Invert && favoriteFilter.FavoriteSetting == FavoriteFilter.FavoriteOption.NoFavorite)
                {
                    conditions.Append(" AND Favorite = 1");
                }
                else if (!favoriteFilter.Invert && favoriteFilter.FavoriteSetting == FavoriteFilter.FavoriteOption.NoFavorite
                    || favoriteFilter.Invert && favoriteFilter.FavoriteSetting == FavoriteFilter.FavoriteOption.OnlyFavorite)
                {
                    conditions.Append(" AND Favorite = 0");
                }
                else if(favoriteFilter.Invert && favoriteFilter.FavoriteSetting == FavoriteFilter.FavoriteOption.All)
                {
                    conditions.Append(" AND 1 = 0");
                }
            }
            else if(filter is PrivateFilter privateFilter)
            {
                if (!privateFilter.Invert && privateFilter.PrivateSetting == PrivateFilter.PrivateOption.OnlyPrivate
                    || privateFilter.Invert && privateFilter.PrivateSetting == PrivateFilter.PrivateOption.NoPrivate)
                {
                    conditions.Append(" AND Private = 1");
                }
                else if (!privateFilter.Invert && privateFilter.PrivateSetting == PrivateFilter.PrivateOption.NoPrivate
                    || privateFilter.Invert && privateFilter.PrivateSetting == PrivateFilter.PrivateOption.OnlyPrivate)
                {
                    conditions.Append(" AND Private = 0");
                }
                else if (privateFilter.Invert && privateFilter.PrivateSetting == PrivateFilter.PrivateOption.All)
                {
                    conditions.Append(" AND 1 = 0");
                }
            }
            else if(filter is MediaTypeFilter mediaTypeFilter)
            {
                conditions.Append(!filter.Invert
                    ? " AND Type = @mediaType"
                    : " AND Type <> @mediaType");
                command.Parameters.AddWithValue("@mediaType", mediaTypeFilter.MediaType);
            }
            else if (filter is TagFilter tagFilter && !string.IsNullOrEmpty(tagFilter.Tag))
            {
                conditions.Append(!filter.Invert
                    ? " AND Tags LIKE "
                    : " AND Tags NOT LIKE ");
                conditions.Append($"'%#{tagFilter.Tag}#%'");
            }
        }

        public async Task<IEnumerable<string>> GetAllTags()
        {
            var tagEntries = new List<string>();
            const string cmdTxt = "SELECT Tags FROM MediaInfo";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                conn.Open();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                        return new List<string>();

                    while (reader.Read())
                        tagEntries.Add(reader.GetString(0));
                }
            }

            return tagEntries.SelectMany(SeparateTags).Distinct();
        }
    }
}
