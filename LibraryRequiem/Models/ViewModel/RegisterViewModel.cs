using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibraryRequiem.Models.ViewModel
{
    public class RegisterViewModel
    {
        [DisplayName("Имя пользователя")]
        [Required(ErrorMessage = "Введите имя пользователя!")]
        public string UserName { get; set; }

        [DisplayName("Пароль")]
        [Required(ErrorMessage = "Введите пароль!")]
        public string Password { get; set; }

        [DisplayName("Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли должны совпадать!")]
        [Required(ErrorMessage = "Введите подтверждение пароля!")]
        public string PasswordConfirm { get; set; }
    }
}
