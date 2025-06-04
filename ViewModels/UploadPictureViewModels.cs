using System.ComponentModel.DataAnnotations;

namespace GalleryApp.ViewModels
{
    public class UploadPictureViewModel
    {
        public IFormFile Image { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
    }
}