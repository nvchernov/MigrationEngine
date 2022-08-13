using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace MigrationEngine.Tests
{
    [Migration(2022, 01, 01, 3, description: "test3")]
    public class TestMigration3 : MigrationBase
    {

        public override void Up()
        {

            using var conn = this.ConnectionFactory.Create();

            conn.Query("insert into Records (id, name) values (3, 'test3');");
        }

        public override void Down()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query("delete from Records where id = 3;");
        }

    }
}
