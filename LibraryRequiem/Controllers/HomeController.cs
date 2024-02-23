using LibraryRequiem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using LibraryRequiem.Data;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.HttpResults;
using LibraryRequiem.Models.ViewModel;

namespace LibraryRequiem.Controllers
{
    public class HomeController : Controller
    {
        //Создание переменных для логирования и контекста базы данных
        private readonly ILogger<HomeController> _logger;
        private readonly CollectionContext _context;

        //Инициализация этих переменных
        public HomeController(ILogger<HomeController> logger, CollectionContext context)
        {
            _context = context;
            _logger = logger;
        }

        //Переопределение метода OnActionExecuting
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Помещение всех полей Genre из базы данных в переменную Genres
            IEnumerable<string> Genres = _context.Books.Select(g => g.Genre);

            //Исключение повторяющихся элементов
            Genres = Genres.Distinct();

            //Помещение Genres во ViewBag
            ViewBag.Genres = Genres;

            //Сохранение изменений
            base.OnActionExecuting(context);
        }

        //Возврат кода ошибки 404
        public IActionResult NotFind() { return StatusCode(404); }
        public async Task<IActionResult> Index()
        {
            //Проверка бд на null
            if (!_context.Books.IsNullOrEmpty())
            {
                var NewBooks = await _context.Books.ToListAsync();
                var viewModel = new CollectionIndexViewModel();

                //Сортировка объектов бд по дате добавления
                NewBooks.Sort((x,y) => x.DateOfUpload.Date.CompareTo(y.DateOfUpload.Date));

                //Выборка последних 4х элементов
                var firstFourEllements = NewBooks.TakeLast(4).Reverse().ToList();

                if (User.Identity.IsAuthenticated)
                {
                    UserModel user = await _context.Users.Include(x => x.FavoriteList.Books)
                    .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

                    viewModel.User = user;
                    viewModel.Books = firstFourEllements;

                    return View(viewModel);
                }

                viewModel.User = null;
                viewModel.Books = firstFourEllements;

                return View(viewModel);
            }

            return View();
        }

        /*[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }*/
    }
}