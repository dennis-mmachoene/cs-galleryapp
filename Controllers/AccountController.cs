using System.Security.Claims;
using BCrypt.Net;
using GalleryApp.Data;
using GalleryApp.Models;
using GalleryApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GalleryApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext appDbContext;

        public AccountController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (await appDbContext.Users.AnyAsync(u => u.Username == viewModel.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken");
                    return View(viewModel);
                }
                if (await appDbContext.Users.AnyAsync(u => u.Email == viewModel.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(viewModel);
                }

                var user = new User
                {
                    Username = viewModel.Username,
                    Email = viewModel.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(viewModel.Password)
                };

                await appDbContext.Users.AddAsync(user);
                await appDbContext.SaveChangesAsync();

                return RedirectToAction("Login");

            }
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await appDbContext.Users.FirstOrDefaultAsync(x => x.Username == viewModel.Username);

                if (user != null && BCrypt.Net.BCrypt.Verify(viewModel.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid username or password");

            }
            return View(viewModel);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await appDbContext.Users.Include(u => u.Pictures).FirstOrDefaultAsync(u => u.Id == userId);

            return View(user);
        }
    }
}