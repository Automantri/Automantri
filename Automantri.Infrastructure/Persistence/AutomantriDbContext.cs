using Automantri.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automantri.Infrastructure.Persistence;

public sealed class AutomantriDbContext(DbContextOptions<AutomantriDbContext> options) : DbContext(options)
{
    public DbSet<Car> Cars => Set<Car>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutomantriDbContext).Assembly);
    }
}
