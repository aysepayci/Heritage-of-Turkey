using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Heritage_of_Turkey.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var favorites = await _context.Favorites
                .AsNoTracking()
                .Include(f => f.Museum)
                    .ThenInclude(m => m.Category)
                .Include(f => f.Ruin)
                    .ThenInclude(r => r.Category)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            var items = favorites
                .Where(f => f.Museum != null || f.Ruin != null)
                .Select(f =>
                {
                    if (f.Museum != null)
                    {
                        return new FavoriteItemViewModel
                        {
                            ItemId = f.Museum.MuseumId,
                            ItemType = "Museum",
                            Title = f.Museum.MuseumName,
                            City = f.Museum.City,
                            District = f.Museum.District,
                            Description = f.Museum.Description,
                            ImageUrl = f.Museum.ImageUrl,
                            TicketPrice = f.Museum.TicketPrice,
                            CategoryName = f.Museum.Category.CategoryName
                        };
                    }

                    return new FavoriteItemViewModel
                    {
                        ItemId = f.Ruin!.RuinId,
                        ItemType = "Ruin",
                        Title = f.Ruin.RuinName,
                        City = f.Ruin.City,
                        District = f.Ruin.District,
                        Description = f.Ruin.Description,
                        ImageUrl = f.Ruin.ImageUrl,
                        TicketPrice = f.Ruin.TicketPrice,
                        CategoryName = f.Ruin.Category.CategoryName
                    };
                })
                .OrderBy(i => i.ItemType)
                .ThenBy(i => i.Title)
                .ToList();

            var viewModel = new FavoriteListViewModel
            {
                Items = items,
                TotalCount = items.Count
            };

            ViewData["Title"] = "My Favorites";
            ViewBag.FavoriteCount = items.Count;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMuseum(int museumId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();

            var museumExists = await _context.Museums
                .AsNoTracking()
                .AnyAsync(m => m.MuseumId == museumId && m.IsActive);

            if (!museumExists)
            {
                TempData["ErrorMessage"] = "The selected museum could not be found.";
                return RedirectSafely(returnUrl);
            }

            var alreadyExists = await _context.Favorites
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.MuseumId == museumId);

            if (alreadyExists)
            {
                TempData["InfoMessage"] = "This museum is already in your favorites.";
                return RedirectSafely(returnUrl);
            }

            _context.Favorites.Add(new Favorite
            {
                UserId = userId,
                MuseumId = museumId,
                RuinId = null
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Museum added to your favorites.";

            return RedirectSafely(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRuin(int ruinId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();

            var ruinExists = await _context.Ruins
                .AsNoTracking()
                .AnyAsync(r => r.RuinId == ruinId && r.IsActive);

            if (!ruinExists)
            {
                TempData["ErrorMessage"] = "The selected ruin could not be found.";
                return RedirectSafely(returnUrl);
            }

            var alreadyExists = await _context.Favorites
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.RuinId == ruinId);

            if (alreadyExists)
            {
                TempData["InfoMessage"] = "This ruin is already in your favorites.";
                return RedirectSafely(returnUrl);
            }

            _context.Favorites.Add(new Favorite
            {
                UserId = userId,
                MuseumId = null,
                RuinId = ruinId
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruin added to your favorites.";

            return RedirectSafely(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMuseum(int museumId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MuseumId == museumId);

            if (favorite == null)
            {
                TempData["InfoMessage"] = "This museum was not in your favorites.";
                return RedirectSafely(returnUrl);
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Museum removed from your favorites.";

            return RedirectSafely(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRuin(int ruinId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RuinId == ruinId);

            if (favorite == null)
            {
                TempData["InfoMessage"] = "This ruin was not in your favorites.";
                return RedirectSafely(returnUrl);
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruin removed from your favorites.";

            return RedirectSafely(returnUrl);
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        private IActionResult RedirectSafely(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}