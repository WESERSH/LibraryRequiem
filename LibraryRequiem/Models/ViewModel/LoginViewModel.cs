using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibraryRequiem.Models.ViewModel
{
    public class LoginViewModel
    {
        [DisplayName("Имя пользователя")]
        [Required(ErrorMessage = "Введите имя пользователя!")]
        public string? UserNameNew { get; set; }

        [DisplayName("Пароль")]
        [Required(ErrorMessage = "Введите пароль!")]
        public string? PasswordNew { get; set; }
    }
}
