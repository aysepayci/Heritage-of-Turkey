using Heritage_of_Turkey.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heritage_of_Turkey.Models
{
    public class Ruin
    {
        [Key]
        public int RuinId { get; set; }

        [Required(ErrorMessage = "Ruin name is required")]
        [StringLength(150, ErrorMessage = "Ruin name cannot exceed 150 characters")]
        public string RuinName { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        public string City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(300)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(300)]
        public string? ImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TicketPrice { get; set; }

        [StringLength(100)]
        public string? OpeningHours { get; set; }

        [StringLength(100)]
        public string? HistoricalPeriod { get; set; }

        [StringLength(500)]
        [Url(ErrorMessage = "Please enter a valid Google Maps URL")]
        public string? GoogleMapsUrl { get; set; }

        public bool IsFeatured { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Foreign Key
        [Required]
        public int CategoryId { get; set; }

        // Navigation Property
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

        public virtual ICollection<RuinReview> Reviews { get; set; } = new List<RuinReview>();
    }
}
