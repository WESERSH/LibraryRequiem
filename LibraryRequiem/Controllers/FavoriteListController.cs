using Humanizer;
using LibraryRequiem.Data;
using LibraryRequiem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;

namespace LibraryRequiem.Controllers
{
    public class FavoriteListController : Controller
    {
        private readonly CollectionContext _context;

        public FavoriteListController(CollectionContext collectionContext)
        {
            _context = collectionContext;
        }

        public async Task<IActionResult> AddTitle(int id, string currentPage)
        {
            // Получение модели пользователя из базы данных, включая связанные сущности "Список избранных книг"
            var user = await _context.Users
                .Include(x => x.FavoriteList.Books)
                .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

            // Проверка, существует ли уже книга в избранном пользователя
            if (user.FavoriteList.Books.FirstOrDefault(x => x.BookId == id) == null)
            {
                // Если книги нет в избранном, добавляем ее
                user.FavoriteList.Books.Add(new FavoriteBookModel
                {
                    BookId = id,
                    FavoriteList = user.FavoriteList,
                    FavoriteListId = user.FavoriteList.Id,
                });

                // Сохранение изменений в базе данных
                await _context.SaveChangesAsync();
            }

            // Перенаправление на страницу профиля
            return Redirect(currentPage);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteTitle(int id)
        {
            // Получение модели пользователя из базы данных, включая связанные сущности "Список избранных книг"
            var user = await _context.Users.Include(x => x.FavoriteList.Books).FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

            // Удаление книги из избранного пользователя
            user.FavoriteList.Books.Remove(user.FavoriteList.Books.FirstOrDefault(x => x.BookId == id));

            // Сохранение изменений в базе данных
            await _context.SaveChangesAsync();

            // Перенаправление на страницу профиля
            return RedirectToAction("Index", "Profile");
        }
    }
}
