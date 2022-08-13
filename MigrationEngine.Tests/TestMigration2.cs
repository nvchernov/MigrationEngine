using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationEngine.Tests
{
    [Migration(2022, 01, 01, 2, description: "test2")]
    public class TestMigration2 : MigrationBase
    {
        public static bool ThrowExceptionOnce { get; set; } = false;

        public override void Up()
        {

            using var conn = this.ConnectionFactory.Create();

            if(ThrowExceptionOnce)
            {
                ThrowExceptionOnce = false;

                throw new IndexOutOfRangeException("!!! THIS IS TEST EXCEPTION !!!");
            }

            conn.Query("insert into Records (id, name) values (2, 'test2');");

        }

        public override void Down()
        {
            using var conn = this.ConnectionFactory.Create();

            if (ThrowExceptionOnce)
            {
                ThrowExceptionOnce = false;

                throw new IndexOutOfRangeException("!!! THIS IS TEST EXCEPTION !!!");
            }

            conn.Query("delete from Records where id = 2;");

        }
    }
}
