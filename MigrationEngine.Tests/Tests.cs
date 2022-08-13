using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MigrationEngine.Sqlite;
using Dapper;
using System.Linq;
using Microsoft.Data.Sqlite;
using Xunit;

namespace MigrationEngine.Tests
{
    
    public class Tests : IDisposable
    {

        /*
         * How it works:
         * We have some migrations
         * 
         * TestMigration1 - this just create test table 'Records'
         * TestMigration2 - add 1 rec to db { "id" 2, "name": "test2"} at date 2022_01_01 02:00:00
         * TestMigration3 - add 1 rec to db { "id" 3, "name": "test3"} at date 2022_01_01 03:00:00
         * TestMigration4 - add 1 rec to db { "id" 4, "name": "test4"} at date 2022_01_01 04:00:00
         * TestMigration5 - add 1 rec to db { "id" 5, "name": "test5"} at date 2022_01_01 05:00:00
         * TestMigration6 - this must be always skipped 
         * 
         * This class uses SQLite DB and every test it create new db file
         * so every test you have clean db
         * 
         *
         * */

        MigrationManager migrationManager;
        string sqliteDbPath = "testdb.db";
        string ComputedDbPath => Path.Combine(Environment.CurrentDirectory, sqliteDbPath);

        public Tests()
        {
            /*
             * this construtor will be called BEFORE each test
             * 
             * */

            if (File.Exists(ComputedDbPath))
                File.Delete(ComputedDbPath);

            File.WriteAllText(ComputedDbPath, string.Empty);

            migrationManager = new SqliteMigrationManager(new[] { typeof(Tests).Assembly }, $"Data Source={ComputedDbPath};");

            using var connection = migrationManager.ConnectionFactory.Create();


        }

        public void Dispose()
        {

            /*
             * this finilizer will be called AFTER each test
             * 
             * */

            SqliteConnection.ClearAllPools();
        }


        [Fact]
        public void TestMigrationUp()
        {
            /*
             * This will create table and run first migraion
             * 
             * */

            migrationManager.Migrate(1);
            migrationManager.Migrate(1);

            using var conn = migrationManager.ConnectionFactory.Create();

            var hasRec = conn.ExecuteScalar<int>("select count(*) from Records");

            Assert.Equal(1, hasRec);

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(2, migrationsCount);

        }

        [Fact]
        public void TestMigrationUpAndThrow()
        {
            /*
             * This will create table and run first migraions
             * 
             * Second migration will throw exeption
             * 
             * */

            TestMigration2.ThrowExceptionOnce = true;
            
            Execute.SkipingExeptions(() => migrationManager.Migrate());

            using var conn = migrationManager.ConnectionFactory.Create();

            var hasRec = conn.ExecuteScalar<int>("select count(*) from Records");

            Assert.Equal(0, hasRec);

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(1, migrationsCount);

        }


        [Fact]
        public void TestMigrationUpAll()
        {
            migrationManager.Migrate();

            using var conn = migrationManager.ConnectionFactory.Create();

            var recCount = conn.ExecuteScalar<int>("select count(*) from Records");

            Assert.True(recCount == 4);

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(5, migrationsCount);

        }

        [Fact]
        public void TestMigrationUpAllTwice()
        {
            migrationManager.Migrate();

            using var conn = migrationManager.ConnectionFactory.Create();

            var recCount = conn.ExecuteScalar<int>("select count(*) from Records");

            Assert.True(recCount == 4);

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(5, migrationsCount);

            migrationManager.Migrate(1);
            recCount = conn.ExecuteScalar<int>("select count(*) from Records");
            Assert.Equal(4, recCount);



        }


        [Fact]
        public void TestMigrationRollback()
        {
            migrationManager.Migrate();

            using var conn = migrationManager.ConnectionFactory.Create();

            migrationManager.Rollback(1);

            var recCount = conn.ExecuteScalar<int>("select count(*) from Records");

            Assert.Equal(3, recCount);

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(4, migrationsCount);
        }

        [Fact]
        public void TestMigrationRollback999()
        {
            /*
             * Migrate all, then Rollback all
             * 
             * */


            migrationManager.Migrate();

            using var conn = migrationManager.ConnectionFactory.Create();

            migrationManager.Rollback(999);

            var recCount = 
                conn.ExecuteScalar<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Records'");

            Assert.Equal(0, recCount);

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(0, migrationsCount);
        }

        [Fact]
        public void TestMigrationRollback999WithException()
        {
            /*
             * Migrate all, then Rollback all
             * 
             * TestMigration2 will throw exception, so we got only 2 migrations on db
             * 
             * */


            migrationManager.Migrate();

            using var conn = migrationManager.ConnectionFactory.Create();

            TestMigration2.ThrowExceptionOnce = true;

            Execute.SkipingExeptions(() => migrationManager.Rollback(999));

            var recCount = conn.ExecuteScalar<int>("select count(*) from Records");

            Assert.Equal(1, recCount);

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(2, migrationsCount);
        }

        [Fact]
        public void TestMigrationRollbackDueDate()
        {

            /*
             * Migrate all, then rollback to 2022_01_01 03:00:00
             * 
             * and we should see only TestMigration2 in db
             * */

            migrationManager.Migrate();

            using var conn = migrationManager.ConnectionFactory.Create();

            migrationManager.Rollback(dueDate: new DateTime(2022, 01, 01, 03, 0, 0));

            var migrationsCount = migrationManager.GetDbMigrations().Length;

            Assert.Equal(2, migrationsCount);


        }

 
    }
}
