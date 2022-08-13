using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace MigrationEngine.Tests
{
    [Migration(2022, 01, 01, 6, description: "test6", skip:true)]
    public class TestMigration6 : MigrationBase
    {

        public override void Up()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query("insert into Records (id, name) values (6, 'test6');");
        }

        public override void Down()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query("delete from Records where id = 6;");
        }

    }
}