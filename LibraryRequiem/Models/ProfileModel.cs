namespace LibraryRequiem.Models
{
    public class ProfileModel
    {
        // Идентификатор профиля
        public int Id { get; set; }

        // Иконка аккаунта пользователя (в виде массива байтов)
        public byte[] AccountIcon { get; set; }

        // Идентификатор пользователя (связь с моделью UserModel)
        public long UserId { get; set; }

        // Пользователь (связь с моделью UserModel)
        public UserModel User { get; set; }
    }
}