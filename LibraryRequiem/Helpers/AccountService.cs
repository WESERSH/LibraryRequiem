using LibraryRequiem.Data;
using LibraryRequiem.Models;
using LibraryRequiem.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryRequiem.Helpers
{
    public class AccountService
    {
        private readonly CollectionContext _context;

        public AccountService(CollectionContext context)
        {
            _context = context;
        }

        public static ClaimsIdentity Authenticate(UserModel user)
        {
            var claims = new List<Claim>()
            {

                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)

            };

            return new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }

        public ClaimsIdentity Login(LoginViewModel model)
        {

            try
            {
                var user = _context.Users.FirstOrDefault(x => x.UserName == model.UserNameNew);

                if (user == null)
                {
                    return null;
                }
                if (user.Password != HashPasswordHelper.HashPassword(model.PasswordNew))
                {
                    return null;
                }

                var result = Authenticate(user);

                return result;
            }

            catch (Exception ex)
            {
                return null;
            }


        }
    }
}
