using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace MigrationEngine.Tests
{

    [Migration(2022, 01, 01, 1, description: "test1")]
    public class TestMigration1 : MigrationBase
    {

        public override void Up()
        {
            using var conn = this.ConnectionFactory.Create();

            conn.Query($@"
create table Records
(
	id int,
	name TEXT
);");

        }

        public override void Down()
        {
            using var conn = this.ConnectionFactory.Create();
            
            conn.Query("drop table Records;");

        }
    }
}
