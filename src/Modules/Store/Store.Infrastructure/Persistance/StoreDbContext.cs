using Microsoft.EntityFrameworkCore;
using Store.Application.Abstractions;

namespace Store.Infrastructure.Persistance
{
    public sealed class StoreDbContext : DbContext, IUnitOfWork
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<Store.Domain.Stores.Store> Stores => Set<Store.Domain.Stores.Store>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
