using System.ComponentModel.DataAnnotations;
using Heritage_of_Turkey.ViewModels.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Heritage_of_Turkey.ViewModels.Admin
{
    public class RuinAdminListViewModel
    {
        public IList<RuinAdminItemViewModel> Ruins { get; set; } = new List<RuinAdminItemViewModel>();

        public int TotalCount { get; set; }

        public int ActiveCount { get; set; }

        public int FeaturedCount { get; set; }

        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
    }

    public class RuinAdminItemViewModel
    {
        public int RuinId { get; set; }

        public string RuinName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string? HistoricalPeriod { get; set; }

        public string? GoogleMapsUrl { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }

    public class RuinAdminFormViewModel
    {
        public int RuinId { get; set; }

        [Required(ErrorMessage = "Ruin name is required")]
        [StringLength(150, ErrorMessage = "Ruin name cannot exceed 150 characters")]
        [Display(Name = "Ruin Name")]
        public string RuinName { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Image URL cannot exceed 300 characters")]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Range(0, 999999, ErrorMessage = "Ticket price must be a positive value")]
        [Display(Name = "Ticket Price")]
        public decimal? TicketPrice { get; set; }

        [StringLength(100, ErrorMessage = "Opening hours cannot exceed 100 characters")]
        [Display(Name = "Opening Hours")]
        public string? OpeningHours { get; set; }

        [StringLength(100, ErrorMessage = "Historical period cannot exceed 100 characters")]
        [Display(Name = "Historical Period")]
        public string? HistoricalPeriod { get; set; }

        [StringLength(500, ErrorMessage = "Google Maps URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid Google Maps URL")]
        [Display(Name = "Google Maps URL")]
        public string? GoogleMapsUrl { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IList<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }

    public class RuinAdminDetailViewModel
    {
        public int RuinId { get; set; }

        public string RuinName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Address { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string? OpeningHours { get; set; }

        public string? HistoricalPeriod { get; set; }

        public string? GoogleMapsUrl { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}
