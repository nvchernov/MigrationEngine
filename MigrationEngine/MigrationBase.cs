using System;
using System.Reflection;

namespace MigrationEngine
{
    /// <summary>
    /// Inherit your migration class of this and mark it with <see cref="MigrationEngine.MigrationAttribute"/>
    /// </summary>
    public abstract class MigrationBase
    {

        public virtual IDbConnectionFactory ConnectionFactory { get; set; }

        public virtual IConfig Config { get; set; }

        private MigrationAttribute migrationAttribute = null;
        private MigrationAttribute MigrationAttribute
        {
            get => migrationAttribute
                ?? (migrationAttribute = ((MigrationAttribute)this.GetType().GetCustomAttribute(typeof(MigrationAttribute))))
                ?? (migrationAttribute = MigrationAttribute.NullObject);
        }

        /// <summary>
        /// Migration creation date
        /// </summary>
        public DateTime CreatedAt => 
            MigrationAttribute.CreatedAt;

        public string Name => 
            this.GetType().Name;

        /// <summary>
        /// Migration description
        /// </summary>
        public string Description =>
            MigrationAttribute.Description;

        public bool Skip => 
            MigrationAttribute.Skip;

        /// <summary>
        /// Logic of migration that remove data or structure modifications 
        /// </summary>
        public abstract void Down();
        
        /// <summary>
        /// Logic of migration that modify data or structure
        /// </summary>
        public abstract void Up();

    }
}
