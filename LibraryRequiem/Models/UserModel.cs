using System.ComponentModel;

namespace LibraryRequiem.Models
{
    public class UserModel
    {
        public int Id { get; set; } // Идентификатор пользователя

        [DisplayName("Имя пользователя")]
        public string UserName { get; set; } // Имя пользователя

        [DisplayName("Пароль")]
        public string Password { get; set; } // Пароль пользователя

        // Роль пользователя
        public string Role { get; set; }

        // Список избранных книг пользователя (связь с моделью FavoriteListModel)
        public virtual FavoriteListModel FavoriteList { get; set; }

        // Профиль пользователя (связь с моделью ProfileModel)
        public ProfileModel Profile { get; set; }
    }
}
