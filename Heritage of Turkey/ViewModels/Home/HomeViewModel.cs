using System.Collections.Generic;

namespace Heritage_of_Turkey.ViewModels.Home
{
    public class HomeViewModel
    {
        public IList<FeaturedMuseumViewModel> FeaturedMuseums { get; set; } = new List<FeaturedMuseumViewModel>();

        public IList<FeaturedRuinViewModel> FeaturedRuins { get; set; } = new List<FeaturedRuinViewModel>();

        public HomeStatisticsViewModel Statistics { get; set; } = new HomeStatisticsViewModel();
    }

    public class FeaturedMuseumViewModel
    {
        public int MuseumId { get; set; }

        public string MuseumName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string? CategoryName { get; set; }
    }

    public class FeaturedRuinViewModel
    {
        public int RuinId { get; set; }

        public string RuinName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string? CategoryName { get; set; }
    }

    public class HomeStatisticsViewModel
    {
        public int MuseumCount { get; set; }

        public int RuinCount { get; set; }

        public int CategoryCount { get; set; }

        public int FeaturedPlaceCount { get; set; }
    }
}
