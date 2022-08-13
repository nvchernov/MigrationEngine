using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace MigrationEngine.Sqlite
{
    public class SqliteMigrationManager : MigrationManager
    {
        private readonly string connectionString;
        private readonly string schemaName;

        private const string defaultTableName = "__migration_engine";

        private string ComputedTableName => $"{defaultTableName}{(string.IsNullOrWhiteSpace(schemaName) ? "" : ".")}";

        public override IDbConnectionFactory ConnectionFactory { get; protected set; }

        bool tableWasCreated = false;

        public SqliteMigrationManager(Assembly[] assemblies, string connectionString, string schemaName = "") : base(assemblies)
        {
            this.connectionString = connectionString;
            this.schemaName = schemaName;
            ConnectionFactory = new SqliteConnectionFactory(connectionString);
        }

        protected override void CreateMigrationEngineTableIfNotExists()
        {

            if (tableWasCreated)
                return;

            var sql = @$"
CREATE TABLE IF NOT EXISTS {ComputedTableName}
(
    id INTEGER PRIMARY KEY,
    migration_date TEXT,
    name TEXT, 
    description TEXT,
    created_at TEXT,
    skip INTEGER
)";

            using var connection = ConnectionFactory.Create();

            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            cmd.ExecuteNonQuery();

            tableWasCreated = true;

        }

        protected override void SaveMigration(MigrationBase migration)
        {

            if (migration is null)
                throw new ArgumentNullException($"param {nameof(migration)} is null");

            if (migration.CreatedAt == DateTime.MinValue || migration.CreatedAt == DateTime.MaxValue)
                throw new ArgumentException("date is invalid");

            // ISO 8601
            var dateAsStandard = StringifyDate(migration.CreatedAt);
            var nowAsString = StringifyDate(DateTime.Now);

            var sql = $@"INSERT INTO {ComputedTableName} 
(migration_date, created_at, name, description, skip) VALUES('{dateAsStandard}','{nowAsString}', '{migration.GetType().Name}', '{migration.Description}', {(migration.Skip ? 1 : 0)});";

            using var connection = ConnectionFactory.Create();

            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            cmd.ExecuteNonQuery();


        }


        protected override void RemoveMigration(MigrationBase migration)
        {
            var sql = $"DELETE FROM {ComputedTableName} WHERE migration_date = '{StringifyDate(migration.CreatedAt)}'";

            using var connection = ConnectionFactory.Create();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            cmd.ExecuteNonQuery();

        }

        public override DbMigration[] GetDbMigrations() =>
            base.GetDbMigrationSQL92();
    }
}
