using System.Diagnostics;
using System.Security.Claims;
using GalleryApp.Data;
using GalleryApp.Models;
using GalleryApp.Services;
using GalleryApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GalleryApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext appDbContext;
        private readonly IImageService imageService;
        private readonly IWebHostEnvironment environment;

        public HomeController(
            AppDbContext appDbContext,
            IImageService imageService,
            IWebHostEnvironment environment
        )
        {
            this.appDbContext = appDbContext;
            this.imageService = imageService;
            this.environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var pictures = await appDbContext
                .Pictures.Include(p => p.User)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();

            return View(pictures);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(UploadPictureViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (imageService.IsValidImage(viewModel.Image))
                {
                    var uploadsPath = Path.Combine(environment.WebRootPath, "uploads");
                    var fileName = await imageService.SaveImageAsync(viewModel.Image, uploadsPath);

                    var picture = new Picture
                    {
                        FileName = fileName,
                        FilePath = Path.Combine("uploads", fileName),
                        Description = viewModel.Description,
                        UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value),
                    };

                    await appDbContext.Pictures.AddAsync(picture);
                    await appDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("Image", "Please select a valid image file under 5MB");
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePicture(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var picture = await appDbContext.Pictures.FirstOrDefaultAsync(p =>
                p.Id == id && p.UserId == userId
            );

            if (picture != null)
            {
                var fullPath = Path.Combine(environment.WebRootPath, picture.FilePath);
                imageService.DeleteImage(fullPath);

                appDbContext.Pictures.Remove(picture);
            }
            return RedirectToAction("Index");
        }
    }
}
