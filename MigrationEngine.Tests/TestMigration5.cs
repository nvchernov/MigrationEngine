using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace MigrationEngine.Tests
{
    [Migration(2022, 01, 01, 5, description: "test5")]
    public class TestMigration5 : MigrationBase
    {

        public override void Up()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query("insert into Records (id, name) values (5, 'test5');");
        }

        public override void Down()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query("delete from Records where id = 5;");
        }

    }
}