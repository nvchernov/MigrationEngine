using System;

namespace MigrationEngine
{
    /// <summary>
    /// You must mark your migration class with this attribute. MigrationEngine uses <see cref="CreatedAt"/> field as identifier of migration (also, it helps to order migrations).
    /// Also you must inherit your migration from <see cref="MigrationBase"/>
    /// </summary>
    public class MigrationAttribute : Attribute
    {
        public static MigrationAttribute NullObject { get; } =
            new MigrationAttribute(0, 0, 0);


        /// <summary>
        /// Migration creation date
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Migration description
        /// </summary>
        public string Description { get; private set; }
        
        public bool Skip { get; private set; }

        /// <summary>
        /// Represents attribute to mark migration class
        /// </summary>
        /// <param name="year">Year when migration was created</param>
        /// <param name="month">Month when migration was created</param>
        /// <param name="day">Day when migration was created</param>
        /// <param name="hour">Hour when migration was created</param>
        /// <param name="minute">Minute when migration was created</param>
        /// <param name="second">Second when migration was created</param>
        /// <param name="description">Description of migration</param>
        public MigrationAttribute(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, string description = null, bool skip = false)
        {
            CreatedAt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
            Description = description;
            Skip = skip;
        }

    }
}
