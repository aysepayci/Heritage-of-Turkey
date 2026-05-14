using System.Collections.Generic;

namespace Heritage_of_Turkey.ViewModels.Favorites
{
    public class FavoriteListViewModel
    {
        public IList<FavoriteItemViewModel> Items { get; set; } = new List<FavoriteItemViewModel>();

        public int TotalCount { get; set; }
    }

    public class FavoriteItemViewModel
    {
        public int ItemId { get; set; }

        public string ItemType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal? TicketPrice { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}