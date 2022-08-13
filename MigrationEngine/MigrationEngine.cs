using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MigrationEngine
{

    public enum ExceptionMode
    {
        Unknown = 0,
        Throw,
        ThrowByUsingEvent
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ExceptionEventArgs(Exception exception) : base()
        {
            Exception = exception;
        }

    }


    internal class MigrationComparer : IComparer<MigrationBase>
    {
        public int Compare(MigrationBase x, MigrationBase y) =>
            DateTime.Compare(x.CreatedAt, y.CreatedAt);
    }

    internal class MigrationEqualityComparer : IEqualityComparer<MigrationBase>
    {
        public bool Equals(MigrationBase x, MigrationBase y) => 
            x?.CreatedAt == y?.CreatedAt;

        public int GetHashCode(MigrationBase obj) => 
            obj.CreatedAt.GetHashCode();
    }
}
