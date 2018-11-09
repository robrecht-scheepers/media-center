using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.Sessions.Staging;

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
            using (var dbConnection = GetConnection())
            {
                using (var command = new SQLiteCommand(dbConnection) {CommandText = createDbSql})
                {
                    dbConnection.Open();
                    command.ExecuteNonQuery(CommandBehavior.CloseConnection);
                }
            }
        }

        public async Task AddMediaInfo(MediaItem newItem)
        {
            const string cmdTxt = 
                "INSERT INTO MediaInfo(Name, Type, Filename, DateTaken, DateAdded, Favorite, Private, Rotation) " +
                "VALUES(@name, @type, @filename, @dateTaken, @dateAdded, @favorite, @private, @rotation);";

            using (var conn = GetConnection())
            using (var command = GetCommand(conn, cmdTxt))
            {
                command.Parameters.AddWithValue("@name", newItem.Name);
                command.Parameters.AddWithValue("@type", newItem.MediaType);
                command.Parameters.AddWithValue("@filename", newItem.ContentFileName);
                command.Parameters.AddWithValue("@dateTaken", newItem.DateTaken);
                command.Parameters.AddWithValue("@dateAdded", newItem.DateAdded);
                command.Parameters.AddWithValue("@favorite", newItem.Favorite);
                command.Parameters.AddWithValue("@private", newItem.Private);
                command.Parameters.AddWithValue("@rotation", newItem.Rotation);

                conn.Open();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
