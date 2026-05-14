using System.ComponentModel.DataAnnotations;

namespace Heritage_of_Turkey.ViewModels.Contact
{
    public class ContactViewModel
    {
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Subject cannot exceed 50 characters")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; } = string.Empty;
    }
}
