using LibraryRequiem.Data;
using LibraryRequiem.Models;
using LibraryRequiem.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryRequiem.Controllers
{
    public class ProfileController : Controller
{
    private readonly CollectionContext _context;

    public ProfileController(CollectionContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
        {
            // Получение модели пользователя из базы данных, включая связанные сущности "Профиль" и "Список избранных книг"
            var userModel = await _context.Users.Include(x => x.Profile)
                .Include(x => x.FavoriteList.Books)
                .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

            // Получение всех книг из базы данных
            var books = _context.Books;

            // Список моделей книг для избранных книг
            List<BookModel> favoriteBooks = new List<BookModel>();

            // Перебор избранных книг пользователя
            foreach (var favoritebook in userModel.FavoriteList.Books)
                {
                    // Получение книги из базы данных по идентификатору из избранного
                    var chosenBook = await books.FirstOrDefaultAsync(x => x.Id == favoritebook.BookId);
                    favoriteBooks.Add(chosenBook);
                }

            // Создание модели представления профиля
            var model = new ProfileViewModel
                {
                    // Имя пользователя
                    ProfileName = User.Identity.Name,

                    // Изображение профиля в виде base64-строки
                    ProfileIcon = "data:image/png;base64, " + Convert.ToBase64String(userModel.Profile.AccountIcon),

                    // Список избранных книг
                    Books = favoriteBooks
                };

            // Возврат представления профиля с моделью
            return View(model);
        }
    }
}
