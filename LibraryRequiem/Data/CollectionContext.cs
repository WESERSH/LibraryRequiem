using Microsoft.EntityFrameworkCore;
using LibraryRequiem.Models;

namespace LibraryRequiem.Data
{
    public class CollectionContext : DbContext
    {
        public CollectionContext(DbContextOptions<CollectionContext> options): base(options) { Database.EnsureCreated(); }

        public DbSet<BookModel> Books { get; set; }

        public DbSet<UserModel> Users { get; set; }
    }
}
