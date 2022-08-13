using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationEngine
{
    /// <summary>
    /// You can get this config form your migrations, this class used by <see cref="MigrationBase"/>
    /// </summary>
    public class Config : IConfig
    {
        public IDictionary<string, string> Values { get; protected set; } = new Dictionary<string, string>();

        public string this[string key] 
        { 
            get => Values.TryGetValue(key, out var val) ? val : null;
        }
    }
}
