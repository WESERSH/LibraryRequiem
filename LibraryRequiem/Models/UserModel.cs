﻿using System.ComponentModel;

namespace LibraryRequiem.Models
{
    public class UserModel
    {
        public int Id { get; set; }


        [DisplayName("Имя пользователя")]
        public string UserName { get; set; }

        [DisplayName("Пароль")]
        public string Password { get; set; }
        public LikedBookModel LikedBook { get; set; }
    }
}