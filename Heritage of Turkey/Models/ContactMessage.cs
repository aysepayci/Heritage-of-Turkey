using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace Heritage_of_Turkey.Models
{
    public enum ContactMessageStatus
    {
        Unread = 0,
        Read = 1,
        Replied = 2
    }

    public class ContactMessage
    {
        [Key]
        public int ContactMessageId { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

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

        [StringLength(1000)]
        public string? AdminReply { get; set; }

        public ContactMessageStatus Status { get; set; } = ContactMessageStatus.Unread;

        public bool IsRead { get; set; } = false;

        public DateTime SentDate { get; set; } = DateTime.Now;

        public DateTime? ReadDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? RepliedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
