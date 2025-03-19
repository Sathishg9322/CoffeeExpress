using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace CoffeeShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string Name,string EmailId, string Password)
        {
            if(string.IsNullOrEmpty(EmailId) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Message = "Email Id and password are required ";
                return View();
            }

            if (_context.Users.Any(u => u.EmailId == EmailId))
            {
                ViewBag.Message = "Username is already exists";
                return View();
            }

            var user = new User
            {
                Name = Name,
                EmailId = EmailId,
                Password = Password,
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Registration successful. You can now log in.";
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string EmailId, string Password)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailId == EmailId && u.Password == Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.EmailId),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimIdentity));

                if (user.Role == "Admin")
                {

                    return RedirectToAction("Index", "Home");
                }

                else if(user.Role == "Customer")
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Message = "Invalid username or password.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

       
    }
}
