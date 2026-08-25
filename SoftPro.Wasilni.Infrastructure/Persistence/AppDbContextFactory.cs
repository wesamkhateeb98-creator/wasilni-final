using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SoftPro.Wasilni.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        bool useMySql = env == "Production";

        DbContextOptions<AppDbContext> options;

        if (useMySql)
        {
            var mysqlConn = Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? "Server=194.5.159.213;Port=3317;Database=Waselni-Database;Uid=admin;Pwd=Kh@led0932aws;";

            options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(mysqlConn, new MySqlServerVersion(new Version(8, 0, 36)),
            mySqlOptions => mySqlOptions
            .CommandTimeout(120)
            .EnableRetryOnFailure(maxRetryCount: 1, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null))
            .Options;
        }
        else
        {
            const string sqlServerConn =
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Wasilni;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

            options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(sqlServerConn)
                .Options;
        }

        return new AppDbContext(options);
    }
}