using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace MigrationEngine.Tests
{
    [Migration(2022, 01, 01, 4, description: "test4")]
    public class TestMigration4 : MigrationBase
    {

        public override void Up()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query("insert into Records (id, name) values (4, 'test4');");
        }

        public override void Down()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query("delete from Records where id = 4;");
        }

    }
}