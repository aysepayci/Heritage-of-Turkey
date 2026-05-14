using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Heritage_of_Turkey.ViewModels.Shared;

namespace Heritage_of_Turkey.ViewModels.Museum
{
    public class MuseumListViewModel
    {
        public IList<MuseumCardViewModel> Museums { get; set; } = new List<MuseumCardViewModel>();

        public IList<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        public string? SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public string? SelectedCategoryName { get; set; }

        public int TotalCount { get; set; }

        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
    }

    public class MuseumCardViewModel
    {
        public int MuseumId { get; set; }

        public string MuseumName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string? OpeningHours { get; set; }

        public bool IsFeatured { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}
