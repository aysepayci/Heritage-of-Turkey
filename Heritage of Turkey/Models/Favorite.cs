using Heritage_of_Turkey.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heritage_of_Turkey.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public string UserId { get; set; }

        public int? MuseumId { get; set; }

        public int? RuinId { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("MuseumId")]
        public virtual Museum? Museum { get; set; }

        [ForeignKey("RuinId")]
        public virtual Ruin? Ruin { get; set; }
    }
}