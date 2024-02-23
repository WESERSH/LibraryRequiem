using System.ComponentModel.DataAnnotations;

namespace LibraryRequiem.Models
{
    public class FavoriteListModel
    {
        public int Id { get; set; } // Идентификатор списка избранных книг

        public UserModel User { get; set; } // Пользователь (связь с моделью UserModel)

        // Идентификатор пользователя (связь с моделью UserModel)

        [MaxLength(int.MaxValue)]
        public long UserId { get; set; }

        // Список избранных книг (связь с моделью FavoriteBookModel)
        public List<FavoriteBookModel> Books { get; set; }
    }
}
