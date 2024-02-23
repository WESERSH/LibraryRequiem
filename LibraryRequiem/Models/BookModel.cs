using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace LibraryRequiem.Models
{
    public class BookModel
    {
        public int Id { get; set; } // Идентификатор книги

        [MaxLength(100)]
        [DisplayName("Название")]
        public string? Title { get; set; } // Название книги

        [MaxLength(50)]
        [DisplayName("Автор")]
        public string? Author { get; set; } // Автор книги

        [DisplayName("Жанры")]
        public string? Genre { get; set; } // Жанры книги
        [MaxLength (500)]
        [DisplayName("Описание")]
        public string? Description { get; set; } // Описание книги

        [DisplayName("Дата загрузки")]
        public DateTime DateOfUpload { get; set; } // Дата загрузки книги

        [DisplayName("Тэги")]
        public string? Tags { get; set; } // Теги книги

        // Путь к файлу книги в формате .fb2
        [DisplayName("Файл книги(.fb2)")]
        public string? BookFilePath { get; set; }

        // Путь к изображению обложки книги в форматах .jpg или .png
        [DisplayName("Обложка(.jpg, .png)")]
        public string? BookImagePath { get; set; }
    }
}
