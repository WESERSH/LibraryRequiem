using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibraryRequiem.Models.ViewModel
{
    public class LoginViewModel
    {
        [DisplayName("Имя пользователя")]
        [Required(ErrorMessage = "Введите имя пользователя!")]
        public string UserName { get; set; }

        [DisplayName("Пароль")]
        [Required(ErrorMessage = "Введите пароль!")]
        public string Password { get; set; }
    }
}
