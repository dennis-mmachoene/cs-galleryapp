namespace GalleryApp.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image, string uploadsPath);
        bool IsValidImage(IFormFile file);
        void DeleteImage(string filePath);
    }
}