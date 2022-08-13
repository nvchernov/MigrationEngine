using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Data.Common;

namespace MigrationEngine
{
    /// <summary>
    /// Manage your migrations here. This class has methods that help you to execute\rollback migrations and other. 
    /// You must call methods of <see cref="MigrationManager"/> ancestor by yourself. It has no auto-things e.g. for asp.net. 
    /// If you use e.g. MySql, you must get MigrationManager.MySql package and use it
    /// </summary>
    public abstract class MigrationManager
    {
        protected readonly Assembly[] assemblies;

        protected readonly Lazy<MigrationBase[]> migrations;

        public abstract IDbConnectionFactory ConnectionFactory { get; protected set; }

        public IConfig Config { get; protected set; } = new Config();

        
        public static string StringifyDate(DateTime value) =>
             value.ToString("o"); 

        public static DateTime? TryParseDate(string value) =>
        DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime res) ? res : (DateTime?)null;

        public static DateTime ParseDate(string value) =>
            DateTime.ParseExact(value, "o", CultureInfo.InvariantCulture);

        public MigrationManager(Assembly[] assemblies, IConfig config = null)
        {
            this.assemblies = assemblies;
            this.migrations = new Lazy<MigrationBase[]>(() => FindMirgrations(this.assemblies));
            this.Config = config;
        }

        #region find migrations
        // ------------------------ ------------------------
        // order by desc - e.g. 10, 7, 6, 4, 2, 1
        private MigrationBase[] FindMirgrations(Assembly[] assemblies)
        {
            if (assemblies?.Any() != true)
                return new MigrationBase[0];

            var migrations = assemblies
                .Select(x => TakeMigrationsOfAssembly(x))
                .SelectMany(x => x)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            migrations.ForEach(x => x.ConnectionFactory = ConnectionFactory);
            migrations.ForEach(x => x.Config = Config);

            return migrations.ToArray();
        }

        private MigrationBase[] TakeMigrationsOfAssembly(Assembly assembly)
        {
            try
            {

                var assemblyMigrationClasses = assembly.GetTypes().Where(x => TypeWasMarkedAsMigration(x)).ToList();
                var migrations = new List<MigrationBase>();

                foreach (var klass in assemblyMigrationClasses)
                {
                    var migration = Activator.CreateInstance(klass) as MigrationBase;

                    if (migration == null)
                        throw new Exception($"Cannot create instance of migration '{klass.Name}' for some reason");

                    if (migration.CreatedAt == null)
                        throw new Exception($"Migration '{klass.Name}' has no migration date at attribute Migration");

                    var sameMigration = migrations.FirstOrDefault(x => x?.CreatedAt == migration.CreatedAt);

                    if (sameMigration != null)
                        throw new Exception(
                            $"Migration '{klass.Name}' same as '{sameMigration.GetType().Name}' migration, dates are equal. Please change date and compile assembly where migration was changed");

                    migrations.Add(migration);
                }

                migrations.Sort(new MigrationComparer());
                return migrations.ToArray();

            }

            catch (Exception ex)
            {
                HandleException(ex);

                return new MigrationBase[0];
            }
        }

        protected void HandleException(Exception ex, MigrationBase migration = null)
        {
            var exception = migration is null
                ? ex 
                : new MigrationException($"migration '{migration.Name}' failed! reason: '{ex.Message}', see inner exeptions for details", ex);

            if (ExceptionMode == ExceptionMode.ThrowByUsingEvent)
                OnExceptionOccured(exception);
            else
                throw exception;
        }

        public bool TypeWasMarkedAsMigration(Type t) =>
            t.IsClass && t.IsPublic &&
            (t.CustomAttributes?.Any(attr => attr.AttributeType.IsEquivalentTo(typeof(MigrationAttribute)) || attr.AttributeType.IsSubclassOf(typeof(MigrationAttribute))) ?? false);

        // ------------------------ ------------------------
        #endregion

        public event EventHandler<ExceptionEventArgs> ExceptionOccured;
        protected virtual void OnExceptionOccured(Exception ex) =>
            ExceptionOccured?.Invoke(this, new ExceptionEventArgs(ex));

        public ExceptionMode ExceptionMode { get; set; } = ExceptionMode.Throw;

        public virtual void Migrate(int step = int.MaxValue)
        {

            MigrationBase migration = null;

            try
            {
                CreateMigrationEngineTableIfNotExists();

                var dbMigrations = GetDbMigrations();
                var assemblyMigrations = migrations.Value;

                if (assemblyMigrations?.Any() != true)
                    return;

                var assemblyMigrationsToRun = assemblyMigrations
                    .Where(x => !x.Skip)
                    .Where(x => !dbMigrations.Any(dbM => dbM.Name?.ToLower() == x.Name?.ToLower()))
                    .OrderBy(x => x.CreatedAt)
                    .Take(step)
                    .ToArray();

                foreach (var item in assemblyMigrationsToRun)
                {
                    migration = item;
                    item.Up();

                    SaveMigration(item);
                }

            }
            catch (Exception ex)
            {
                HandleException(ex, migration);
                return;
            }
        }

        public virtual void Rollback(int step = int.MaxValue, DateTime dueDate = default)
        {
            MigrationBase migration = null;

            try
            {
                CreateMigrationEngineTableIfNotExists();

                var dbMigrations = GetDbMigrations()
                    .OrderByDescending(x => x.MigrationDate)
                    .Take(step)
                    .ToArray();

                var migrationsToRemoveLinq = GetAssembliesMigrations()
                    .Where(x => !x.Skip)
                    .Where(x => dbMigrations.Any(dbm => dbm.MigrationDate == StringifyDate(x.CreatedAt)));

                if (dueDate != default)
                    migrationsToRemoveLinq = migrationsToRemoveLinq.Where(x => x.CreatedAt >= dueDate);

                var migrationsToRemove = migrationsToRemoveLinq.ToArray();

                foreach (var item in migrationsToRemove)
                {
                    migration = item;

                    item.Down();
                    RemoveMigration(item);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, migration);
            }
        }

        /// <summary>
        /// All DB migration migrations
        /// </summary>
        public abstract DbMigration[] GetDbMigrations();

        public virtual MigrationBase[] GetAssembliesMigrations() =>
            migrations.Value;

        protected DbMigration[] GetDbMigrationSQL92()
        {
            var sql = @"select me.id, me.migration_date, me.name, me.description, me.created_at, me.skip from __migration_engine me;";

            var recList = new List<DbMigration>();

            using (var connection = ConnectionFactory.Create())
            {

                var cmd = connection.CreateCommand();

                cmd.CommandText = sql;
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var rec = new DbMigration();

                    rec.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    rec.MigrationDate = reader.IsDBNull(1) ? null : reader.GetString(1);
                    rec.Name = reader.IsDBNull(2) ? null : reader.GetString(2);
                    rec.Description = reader.IsDBNull(3) ? null : reader.GetString(3);
                    rec.CreatedAt = reader.IsDBNull(4) ? null : reader.GetString(4);
                    rec.Skip = reader.IsDBNull(5) ? false : reader.GetInt32(5) > 0;

                    recList.Add(rec);
                }

            }

            return recList.ToArray();
        }

        protected abstract void SaveMigration(MigrationBase migration);

        protected abstract void RemoveMigration(MigrationBase migration);

        protected abstract void CreateMigrationEngineTableIfNotExists();

    }
}
