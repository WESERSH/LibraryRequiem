using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryRequiem.Data
{
    public class DbContextFactory<CollectionContext>
        : IDbContextFactory<CollectionContext> where CollectionContext : DbContext
    {
        private readonly IServiceProvider provider;

        public DbContextFactory(IServiceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(
                $"{nameof(provider)}: You must configure an instance of " +
                "IServiceProvider");
        }

        public CollectionContext CreateDbContext() =>
            ActivatorUtilities.CreateInstance<CollectionContext>(provider);
    }
}