using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace MigrationEngine.MySql
{

    public class MySqlMigrationManager : MigrationManager
    {
        private readonly string connectionString;
        private readonly string schemaName;

        public override IDbConnectionFactory ConnectionFactory { get; protected set; }

        private const string defaultTableName = "__migration_engine";

        protected virtual string ComputedTableName => $"{schemaName}{(string.IsNullOrWhiteSpace(schemaName) ? "" : ".")}{defaultTableName}";

        bool tableWasCreated = false;

        public MySqlMigrationManager(Assembly[] assemblies, string connectionString, IConfig config = null, string schemaName = "") : base(assemblies, config)
        {
            this.connectionString = connectionString;
            this.schemaName = schemaName;
            this.ConnectionFactory = new MySqlConnectionFactory(connectionString);
        }

        protected override void CreateMigrationEngineTableIfNotExists()
        {

            if (tableWasCreated)
                return;

            var checkTableExistsSql = $@"
SELECT count(*)
FROM information_schema.tables
WHERE table_schema = '{schemaName}'
    AND table_name = '{defaultTableName}'
LIMIT 1;";

            var sql = @$"
CREATE TABLE IF NOT EXISTS {ComputedTableName}
(
    id int,
    migration_date VARCHAR(255),
    name VARCHAR(255),
    description VARCHAR(2048),
    created_at VARCHAR(255),
    skip int
);

alter table {ComputedTableName} add constraint {defaultTableName}_pk primary key(id);
alter table {ComputedTableName} modify id int auto_increment;";

            using var connection = ConnectionFactory.Create();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = checkTableExistsSql;

                tableWasCreated = int.TryParse(cmd.ExecuteScalar().ToString(), out var val) ? val == 1 : throw new Exception();
            }

            if(!tableWasCreated)
                using (var cmd = connection.CreateCommand())
                {

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    tableWasCreated = true;
                }

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

            using var cmd = connection.CreateCommand();
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
