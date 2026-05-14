using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var featuredMuseums = await _context.Museums
                .AsNoTracking()
                .Include(m => m.Category)
                .Where(m => m.IsActive && m.IsFeatured)
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => new FeaturedMuseumViewModel
                {
                    MuseumId = m.MuseumId,
                    MuseumName = m.MuseumName,
                    City = m.City,
                    District = m.District,
                    Description = m.Description,
                    ImageUrl = m.ImageUrl,
                    TicketPrice = m.TicketPrice,
                    CategoryName = m.Category.CategoryName
                })
                .Take(3)
                .ToListAsync();

            if (!featuredMuseums.Any())
            {
                featuredMuseums = await _context.Museums
                    .AsNoTracking()
                    .Include(m => m.Category)
                    .Where(m => m.IsActive)
                    .OrderByDescending(m => m.CreatedDate)
                    .Select(m => new FeaturedMuseumViewModel
                    {
                        MuseumId = m.MuseumId,
                        MuseumName = m.MuseumName,
                        City = m.City,
                        District = m.District,
                        Description = m.Description,
                        ImageUrl = m.ImageUrl,
                        TicketPrice = m.TicketPrice,
                        CategoryName = m.Category.CategoryName
                    })
                    .Take(3)
                    .ToListAsync();
            }

            var featuredRuins = await _context.Ruins
                .AsNoTracking()
                .Include(r => r.Category)
                .Where(r => r.IsActive && r.IsFeatured)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new FeaturedRuinViewModel
                {
                    RuinId = r.RuinId,
                    RuinName = r.RuinName,
                    City = r.City,
                    District = r.District,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    TicketPrice = r.TicketPrice,
                    CategoryName = r.Category.CategoryName
                })
                .Take(3)
                .ToListAsync();

            if (!featuredRuins.Any())
            {
                featuredRuins = await _context.Ruins
                    .AsNoTracking()
                    .Include(r => r.Category)
                    .Where(r => r.IsActive)
                    .OrderByDescending(r => r.CreatedDate)
                    .Select(r => new FeaturedRuinViewModel
                    {
                        RuinId = r.RuinId,
                        RuinName = r.RuinName,
                        City = r.City,
                        District = r.District,
                        Description = r.Description,
                        ImageUrl = r.ImageUrl,
                        TicketPrice = r.TicketPrice,
                        CategoryName = r.Category.CategoryName
                    })
                    .Take(3)
                    .ToListAsync();
            }

            var museumCount = await _context.Museums
                .AsNoTracking()
                .CountAsync(m => m.IsActive);

            var ruinCount = await _context.Ruins
                .AsNoTracking()
                .CountAsync(r => r.IsActive);

            var categoryCount = await _context.Categories
                .AsNoTracking()
                .CountAsync(c => c.IsActive);

            var featuredMuseumCount = await _context.Museums
                .AsNoTracking()
                .CountAsync(m => m.IsActive && m.IsFeatured);

            var featuredRuinCount = await _context.Ruins
                .AsNoTracking()
                .CountAsync(r => r.IsActive && r.IsFeatured);

            var viewModel = new HomeViewModel
            {
                FeaturedMuseums = featuredMuseums,
                FeaturedRuins = featuredRuins,
                Statistics = new HomeStatisticsViewModel
                {
                    MuseumCount = museumCount,
                    RuinCount = ruinCount,
                    CategoryCount = categoryCount,
                    FeaturedPlaceCount = featuredMuseumCount + featuredRuinCount
                }
            };

            ViewBag.PageDescription = "Explore Turkey's museums, ancient cities, and cultural heritage routes.";

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult About()
        {
            ViewData["Title"] = "About";
            ViewBag.PageDescription = "Learn more about Heritage of Turkey.";

            return View();
        }
    }
}
