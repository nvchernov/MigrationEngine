# MigrationEngine
Simple migration engine for .net C#

Main idea of project is to make simplest possible migration engine.

## How to use this lib?

### Create migrations

```csharp
    // write date of migration to keep sequence of migrations execution
    [Migration(2022, 02, 02, 22, 00, description: "some description")] 
    public class SomeMigration : MigrationBase
    {
        public override void Up()
        {

          // ... any code ...

          using var conn = this.ConnectionFactory.Create();

          //Dapper example
          conn.Query("insert into some_table (id, name) values (1, 'cool');");

          // ... any code ...

      }

        public override void Down()
        {
            
          // ... any code ...

        }
    }
```

### Run migrations

```csharp
  var migrationManager = 
      new SqliteMigrationManager(new[] { typeof(SomeTypeInYourAssemblyWhereYouHaveMigrations).Assembly }, $" ... ");
  migrationManager.Migrate();
```

In future I want to implement console commands to list, run migrations and etc
