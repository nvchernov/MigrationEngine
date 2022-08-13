using MySqlConnector;
using System;
using System.Data;

namespace MigrationEngine.MySql
{
    internal class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string connectionString;

        public MySqlConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException();
        }

        public IDbConnection Create()
        {
            var connecion = new MySqlConnection(connectionString);
            connecion.Open();

            return connecion;


        }
    }
}
