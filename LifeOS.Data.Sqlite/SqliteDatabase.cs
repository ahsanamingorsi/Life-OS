using LifeOS.MVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LifeOS.Data.Sqlite
{
    /// <summary>
    /// SQLite setup for the Windows app: each computer keeps its own local database file.
    /// The SQLite migrations live in this project; the website keeps its own SQL Server migrations.
    /// </summary>
    public static class SqliteDatabase
    {
        public const string FileName = "lifeos.db";

        public static void Configure(DbContextOptionsBuilder options, string databasePath) =>
            options.UseSqlite($"Data Source={databasePath}",
                sqlite => sqlite.MigrationsAssembly(typeof(SqliteDatabase).Assembly.GetName().Name));

        /// <summary>Creates the database on first run and applies any newer migrations.</summary>
        public static void Migrate(AppDbContext db) => db.Database.Migrate();
    }

    /// <summary>Used only by the EF Core tools (dotnet ef) to create SQLite migrations.</summary>
    public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            SqliteDatabase.Configure(options, Path.Combine(Path.GetTempPath(), "lifeos-design.db"));
            return new AppDbContext(options.Options);
        }
    }
}
