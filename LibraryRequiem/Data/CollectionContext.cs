using Microsoft.EntityFrameworkCore;
using LibraryRequiem.Models;

namespace LibraryRequiem.Data
{
    public class CollectionContext : DbContext
    {
        public CollectionContext(DbContextOptions<CollectionContext> options) : base(options)
        {
            // Проверяем наличие базы данных и автоматически создаем ее, если она отсутствует
            Database.EnsureCreated();
        }

        public DbSet<BookModel> Books { get; set; }

        public DbSet<FavoriteListModel> FavoriteLists { get; set; }

        public DbSet<FavoriteBookModel> LikedBook { get; set; }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<ProfileModel> Profiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация отношений между моделями данных
            modelBuilder.Entity<UserModel>(builder =>
            {
                // Устанавливаем связь один к одному между UserModel и ProfileModel
                builder.HasOne(x => x.Profile)
                    .WithOne(x => x.User)
                    .HasPrincipalKey<UserModel>(x => x.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                // Устанавливаем связь один к одному между UserModel и FavoriteListModel
                builder.HasOne(x => x.FavoriteList)
                    .WithOne(x => x.User)
                    .HasPrincipalKey<UserModel>(x => x.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Один ко многим: FavoriteBookModel и FavoriteListModel
            modelBuilder.Entity<FavoriteBookModel>(builder =>
            {
                // Устанавливаем связь один ко многим между FavoriteBookModel и FavoriteListModel
                builder.HasOne(x => x.FavoriteList)
                    .WithMany(x => x.Books)
                    .HasForeignKey(x => x.FavoriteListId);
            });
        }
    }
}
