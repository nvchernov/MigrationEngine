using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MigrationEngine.Sqlite
{
    internal class SqliteConnectionFactory : IDbConnectionFactory
    {
        private readonly string connectionString;

        public SqliteConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection Create()
        {
            var conn = new SqliteConnection(connectionString);

            conn.Open();

            return conn;
        }
    }
}
