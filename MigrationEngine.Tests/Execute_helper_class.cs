using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationEngine.Tests
{
    /// <summary>
    /// Just help class
    /// </summary>
    internal static class Execute
    {
        public static T SkipingExeptions<T>(Func<T> func) 
        {
            try
            {
                return func.Invoke();
            }
            catch(Exception ex)
            {

            }

            return default;
        }


        public static void SkipingExeptions(Action func)
        {
            try
            {
                func.Invoke();
            }
            catch (Exception ex)
            {

            }
        }

    }
}
