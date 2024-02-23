using LibraryRequiem.Data;
using LibraryRequiem.Helpers;
using LibraryRequiem.Models;
using LibraryRequiem.Models.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryRequiem.Controllers
{
    public class AccountsController : Controller
    {

        private readonly CollectionContext _context;
        private readonly IWebHostEnvironment _environment;

        public AccountsController(CollectionContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public ActionResult Register()
        {
            return View();
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel? model)
        {
            // Проверка модели на валидность
            if (ModelState.IsValid)
            {
                // Поиск пользователя с указанным именем в базе данных
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == model.UserName);

                // Если пользователь с таким именем уже существует, добавляем ошибку в модель
                if (user != null)
                {
                    ModelState.AddModelError("", "Пользователь с данным именем уже существует!!!");
                    return View();
                }

                // Создание нового пользователя
                user = new UserModel
                {
                    // Имя пользователя
                    UserName = model.UserName,

                    // Роль пользователя
                    Role = "user",

                    // Зашифрованный пароль
                    Password = HashPasswordHelper.HashPassword(model.Password)
                };        
                
                // Добавление нового пользователя в контекст базы данных
                _context.Users.Add(user);

                // Сохранение изменений в базе данных
                await _context.SaveChangesAsync();

                // Аутентификация пользователя
                var result = AccountService.Authenticate(user);

                // Попытка авторизации пользователя
                try
                {
                    // Авторизация пользователя с использованием схемы аутентификации по куки
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(result));

                    // Создание списка избранных книг для пользователя
                    user.FavoriteList = new FavoriteListModel()
                    {
                        Books = new List<FavoriteBookModel>(),

                        User = user,

                        UserId = user.Id,
                    };

                    // Создание профиля пользователя
                    user.Profile = new ProfileModel()
                    {
                        UserId = user.Id,

                        User = user,

                        // Установка иконки профиля по умолчанию
                        AccountIcon = System.IO.File.ReadAllBytes(_environment.WebRootPath + "/images/Defaulticon.png")
                    };

                    // Обновление данных пользователя в контексте базы данных
                    _context.Users.Update(user);

                    // Сохранение изменений в базе данных
                    await _context.SaveChangesAsync();

                    // Перенаправление на главную страницу
                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    // Добавление ошибки в модель в случае возникновения исключения
                    ModelState.AddModelError("", "Внутренняя ошибка");
                }

            }
            return View(model);
        }

        public ActionResult Login()
        {
            // Отображение частичного представления формы входа
            return PartialView();
        }

        // POST: AccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            // Проверка модели на валидность
            if (ModelState.IsValid)
            {
                // Попытка входа в систему с использованием сервиса аутентификации
                var result = new AccountService(_context).Login(model);

                // Если вход в систему не удался, добавляем ошибку в модель
                if (result == null)
                {
                    ModelState.AddModelError("", "Неверный логин или пароль");

                    // Возвращаем частичное представление формы входа
                    return PartialView();
                }

                // Авторизация пользователя с использованием схемы аутентификации по кукам
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(result));

                // Перенаправление на главную страницу
                return RedirectToAction("Index", "Home");
            }

            // Возвращаем частичное представление формы входа в случае невалидной модели
            return PartialView("../Accounts/Login");
        }

        public ActionResult Logout(int id)
        {
            // Проверка того, что пользователь авторизован
            if (User.Identity.Name != null)
            {
                // Выход из системы с использованием схемы аутентификации по кукам
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Перенаправление на главную страницу
                return RedirectToAction("Index", "Home");
            }

            // Возвращение представления выхода из системы в случае, если пользователь не авторизован
            return View();
        }
    }
}
