using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediaCenter.Media;

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

            SQLiteConnection.CreateFile(dbPath);
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, createDbSql))
            {
                conn.Open();
                command.ExecuteNonQuery(CommandBehavior.CloseConnection);
            }
        }

        public async Task AddMediaInfo(MediaItem item)
        {
            const string cmdTxt = 
                "INSERT INTO MediaInfo(Name, Type, Filename, DateTaken, DateAdded, Favorite, Private, Rotation, Tags) " +
                "VALUES(@name, @type, @filename, @dateTaken, @dateAdded, @favorite, @private, @rotation, @tags);";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                LoadCommandParameters(command, item);
                conn.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateMediaInfo(MediaItem item)
        {
            const string cmdTxt =
                "UPDATE MediaInfo " +
                "SET Name = @name, Type = @type, Filename = @filename = @filename, DateTaken = @dateTaken, DateAdded = @dateAdded, Favorite = @favorite, Private = @private, Rotation = @rotation, Tags = @tags " +
                "WHERE Id = @Id;";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                LoadCommandParameters(command, item);
                conn.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteMediaInfo(MediaItem item)
        {
            const string cmdTxt =
                "DELETE FROM MediaInfo WHENE Id = @Id;";
            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
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
            command.Parameters.AddWithValue("@tags", AggregateTags(item.Tags));
        }

        private string AggregateTags(IEnumerable<string> tags)
        {
            return tags.Aggregate((a, n) => a + (string.IsNullOrEmpty(a) ? "" : "#") + n);
        }

        private List<string> SeparateTags(string aggregatedTags)
        {
            return aggregatedTags.Split('#').ToList();
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
    }
}
