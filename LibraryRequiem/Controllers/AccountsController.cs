using LibraryRequiem.Data;
using LibraryRequiem.Helpers;
using LibraryRequiem.Models;
using LibraryRequiem.Models.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public AccountsController(CollectionContext context)
        {
            _context = context;
        }


        // GET: AccountController
        public ActionResult Index()
        {
            var model = _context.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);

            return View(model);
        }

        // GET: AccountController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AccountController/Create
        public ActionResult Register()
        {
            return View();
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel? model)
        {

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == model.UserName);
                if (user != null)
                {
                    ModelState.AddModelError("error", "Пользователь с данным именем уже существует");
                    return RedirectToAction(nameof(Register), model);
                }

                user = new UserModel
                {
                    UserName = model.UserName,
                    Role = "user",
                    Password = HashPasswordHelper.HashPassword(model.Password),
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                var result = AccountService.Authenticate(user);

                try
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(result));

                    return RedirectToAction("Index", "Accounts");
                }
                catch
                {
                    ModelState.AddModelError("", "Внутренняя ошибка");
                }


            }           
            
            /*_context.Users.Add(userModel);
            await _context.SaveChangesAsync();*/
            return View(model);
        }

        // POST: AccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string UserName, string Password)
        {
            LoginViewModel model = new LoginViewModel();
            model.UserName = UserName;
            model.Password = Password;

            if (ModelState.IsValid)
            {
                var result = new AccountService(_context).Login(model);

                if (result == null)
                {
                    return RedirectToAction("Index", "Home");

                }

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(result));

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Logout(int id)
        {
            if (User.Identity.Name != null)
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}
