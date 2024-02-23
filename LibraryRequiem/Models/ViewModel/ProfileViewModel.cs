using System.ComponentModel;

namespace LibraryRequiem.Models.ViewModel
{
    public class ProfileViewModel
    {

        [DisplayName("Изображение профиля")]
        public string ProfileIcon { get; set; }

        [DisplayName("Логин пользовтеля")]
        public string ProfileName { get; set; }

        [DisplayName("Список избранного")]
        public List<BookModel> Books { get; set; }
    }
}
