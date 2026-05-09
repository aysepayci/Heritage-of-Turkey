using System.ComponentModel.DataAnnotations;

namespace Heritage_of_Turkey.Models
{
    public class ContactMessage
    {
        [Key]
        public int ContactMessageId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [StringLength(50)]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000)]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SentDate { get; set; } = DateTime.Now;

        public DateTime? ReadDate { get; set; }
    }
}