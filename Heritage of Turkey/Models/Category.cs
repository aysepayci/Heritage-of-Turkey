using System.ComponentModel.DataAnnotations;

namespace Heritage_of_Turkey.Models
{       
    public class Category
        {
            [Key]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Category name is required")]
            [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
            public string CategoryName { get; set; }

            [StringLength(500)]
            public string? Description { get; set; }

            public bool IsActive { get; set; } = true;

            public DateTime CreatedDate { get; set; } = DateTime.Now;

            // Navigation Properties
            public virtual ICollection<Museum> Museums { get; set; }
            public virtual ICollection<Ruin> Ruins { get; set; }
        }
    }