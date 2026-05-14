using System.ComponentModel.DataAnnotations;

namespace Heritage_of_Turkey.ViewModels.Reviews
{
    public class ReviewCreateViewModel
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Comment must be between 3 and 1000 characters")]
        [Display(Name = "Comment")]
        public string CommentText { get; set; } = string.Empty;
    }

    public class ReviewItemViewModel
    {
        public string UserEmail { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
