using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationEngine
{
    /// <summary>
    /// You can get this <see cref="IDbConnectionFactory"/> form your migrations, this class used by <see cref="MigrationBase"/>
    /// If you trying to impliment this by yourself, keep in mind that method Create must return new opened connection
    /// </summary>
    public interface IDbConnectionFactory
    {
        IDbConnection Create();

    }
}
