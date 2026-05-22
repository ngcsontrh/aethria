using Microsoft.EntityFrameworkCore.Design;

namespace Aethria.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=Aethria;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.UseVector());

        return new AppDbContext(optionsBuilder.Options);
    }
}
