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
        public int Id { get; set; }

        [DisplayName("Название")]
        public string? Title { get; set; }

        [DisplayName("Автор")]
        public string? Author { get; set; }

        [DisplayName("Жанры")]
        public string? Genre { get; set; }

        [DisplayName("Описание")]
        public string? Description { get; set; }

        [DisplayName("Дата загрузки")]
        public DateTime DateOfUpload { get; set; }

        [DisplayName("Тэги")]
        public string? Tags { get; set; }

        [DisplayName("Файл книги(.fb2)")]
        public string? BookFilePath { get; set; }

        [DisplayName("Обложка(.jpg, .png)")]
        public string? BookImagePath { get; set; }
    }
}
