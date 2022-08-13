using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationEngine
{
    public class MigrationException : Exception
    {

        public MigrationException() : base()
        {

        }

        public MigrationException(string message) : base(message)
        {

        }

        public MigrationException(string message, Exception exception) : base(message, exception)
        {

        }

    }
}
