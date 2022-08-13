using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationEngine
{
    /// <summary>
    /// Config, that you can access from your migration. 
    /// Basic implimentation <see cref="MigrationEngine.Config"/>
    /// </summary>
    public interface IConfig
    {
        IDictionary<string, string> Values { get; }

        string this[string key] { get; }

    }
}
