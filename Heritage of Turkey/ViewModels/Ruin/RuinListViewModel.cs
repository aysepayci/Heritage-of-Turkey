using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Heritage_of_Turkey.ViewModels.Shared;

namespace Heritage_of_Turkey.ViewModels.Ruin
{
    public class RuinListViewModel
    {
        public IList<RuinCardViewModel> Ruins { get; set; } = new List<RuinCardViewModel>();

        public IList<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public string? SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public string? SelectedCategoryName { get; set; }

        public int TotalCount { get; set; }

        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
    }

    public class RuinCardViewModel
    {
        public int RuinId { get; set; }

        public string RuinName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string? OpeningHours { get; set; }
        public string? HistoricalPeriod { get; set; }

        public bool IsFeatured { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}
