using Heritage_of_Turkey.ViewModels.Reviews;

namespace Heritage_of_Turkey.ViewModels.Museum
{
    public class MuseumDetailViewModel
    {
        public int MuseumId { get; set; }

        public string MuseumName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Address { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string? OpeningHours { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Website { get; set; }

        public string? GoogleMapsUrl { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsFavorite { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public double? AverageRating { get; set; }

        public int ReviewCount { get; set; }

        public IList<ReviewItemViewModel> Reviews { get; set; } = new List<ReviewItemViewModel>();

        public ReviewCreateViewModel NewReview { get; set; } = new ReviewCreateViewModel();
    }
}
