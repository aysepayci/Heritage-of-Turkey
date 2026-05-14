using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("~/Admin", Name = "AdminRoot")]
        [HttpGet("", Name = "AdminDashboard")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var recentMessages = await _context.ContactMessages
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .Select(m => new RecentContactMessageViewModel
                {
                    ContactMessageId = m.ContactMessageId,
                    Name = m.Name,
                    Email = m.Email,
                    Subject = m.Subject,
                    IsRead = m.IsRead,
                    Status = m.Status,
                    SentDate = m.SentDate,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalMuseums = await _context.Museums.AsNoTracking().CountAsync(),
                ActiveMuseums = await _context.Museums.AsNoTracking().CountAsync(m => m.IsActive),
                FeaturedMuseums = await _context.Museums.AsNoTracking().CountAsync(m => m.IsFeatured),
                TotalRuins = await _context.Ruins.AsNoTracking().CountAsync(),
                ActiveRuins = await _context.Ruins.AsNoTracking().CountAsync(r => r.IsActive),
                FeaturedRuins = await _context.Ruins.AsNoTracking().CountAsync(r => r.IsFeatured),
                TotalCategories = await _context.Categories.AsNoTracking().CountAsync(),
                ActiveCategories = await _context.Categories.AsNoTracking().CountAsync(c => c.IsActive),
                TotalFavorites = await _context.Favorites.AsNoTracking().CountAsync(),
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(),
                TotalContactMessages = await _context.ContactMessages.AsNoTracking().CountAsync(),
                UnreadContactMessages = await _context.ContactMessages.AsNoTracking().CountAsync(m => m.Status == ContactMessageStatus.Unread),
                RecentContactMessages = recentMessages
            };

            ViewData["Title"] = "Dashboard";
            ViewBag.UnreadContactMessages = viewModel.UnreadContactMessages;

            return View("~/Views/Admin/Dashboard/Index.cshtml", viewModel);
        }
    }
}
