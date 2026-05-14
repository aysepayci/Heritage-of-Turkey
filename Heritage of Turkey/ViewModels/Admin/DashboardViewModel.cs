using System.Collections.Generic;

using Heritage_of_Turkey.Models;

namespace Heritage_of_Turkey.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalMuseums { get; set; }

        public int ActiveMuseums { get; set; }

        public int FeaturedMuseums { get; set; }

        public int TotalRuins { get; set; }

        public int ActiveRuins { get; set; }

        public int FeaturedRuins { get; set; }

        public int TotalCategories { get; set; }

        public int ActiveCategories { get; set; }

        public int TotalFavorites { get; set; }

        public int TotalUsers { get; set; }

        public int TotalContactMessages { get; set; }

        public int UnreadContactMessages { get; set; }

        public IList<RecentContactMessageViewModel> RecentContactMessages { get; set; } = new List<RecentContactMessageViewModel>();
    }

    public class RecentContactMessageViewModel
    {
        public int ContactMessageId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Subject { get; set; }

        public bool IsRead { get; set; }

        public ContactMessageStatus Status { get; set; }

        public DateTime SentDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
