namespace LibraryRequiem.Models
{
    public class FavoriteBookModel
    {
        // Идентификатор избранной книги
        public int Id { get; set; } 

        // Идентификатор книги (связь с моделью BookModel)
        public long BookId { get; set; } 

        // Список избранных книг (связь с моделью FavoriteListModel)
        public FavoriteListModel FavoriteList { get; set; }

        // Идентификатор списка избранных книг (связь с моделью FavoriteListModel)
        public int FavoriteListId { get; set; } 
    }
}
