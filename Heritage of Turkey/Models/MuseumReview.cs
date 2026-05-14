using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heritage_of_Turkey.Models
{
    public class MuseumReview
    {
        [Key]
        public int MuseumReviewId { get; set; }

        [Required]
        public int MuseumId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string UserEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Comment must be between 3 and 1000 characters")]
        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("MuseumId")]
        public virtual Museum Museum { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
