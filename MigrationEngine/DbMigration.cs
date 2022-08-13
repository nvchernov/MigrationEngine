using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationEngine
{
    /// <summary>
    /// Represents internal <see cref="MigrationEngine"/> table structure. It contains data about executed migrations 
    /// </summary>
    public class DbMigration
    {
        public int Id { get; set; }
        public string MigrationDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }

        /// <summary>
        /// While this field has 'true' value, migration will not apply
        /// </summary>
        public bool Skip { get; set; }

    }
}
