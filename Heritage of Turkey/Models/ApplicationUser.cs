using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Heritage_of_Turkey.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string? ProfilePicture { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual ICollection<Favorite> Favorites { get; set; }

        public virtual ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
    }
}
