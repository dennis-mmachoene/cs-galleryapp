using System.ComponentModel.DataAnnotations;

namespace GalleryApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreadtedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();
    }
}