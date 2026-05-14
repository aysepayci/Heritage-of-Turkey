using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Reviews;
using Heritage_of_Turkey.ViewModels.Ruin;
using Heritage_of_Turkey.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Heritage_of_Turkey.Controllers
{
    public class RuinController : Controller
    {
        private const int PublicPageSize = 6;
        private readonly ApplicationDbContext _context;

        public RuinController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int page = 1)
        {
            ViewData["Title"] = "Ruins";
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;

            page = Math.Max(page, 1);

            var categoryItems = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName,
                    Selected = categoryId.HasValue && c.CategoryId == categoryId.Value
                })
                .ToListAsync();

            var ruinsQuery = _context.Ruins
                .AsNoTracking()
                .Include(r => r.Category)
                .Where(r => r.IsActive);

            if (categoryId.HasValue)
            {
                ruinsQuery = ruinsQuery.Where(r => r.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var keyword = searchTerm.Trim();
                var searchPattern = $"%{keyword}%";

                ruinsQuery = ruinsQuery.Where(r =>
                    EF.Functions.Like(r.RuinName, searchPattern) ||
                    EF.Functions.Like(r.City, searchPattern) ||
                    (r.District != null && EF.Functions.Like(r.District, searchPattern)) ||
                    EF.Functions.Like(r.Address, searchPattern) ||
                    EF.Functions.Like(r.Description, searchPattern) ||
                    EF.Functions.Like(r.Category.CategoryName, searchPattern));
            }

            var totalCount = await ruinsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PublicPageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var ruins = await ruinsQuery
                .OrderByDescending(r => r.IsFeatured)
                .ThenBy(r => r.RuinName)
                .Skip((page - 1) * PublicPageSize)
                .Take(PublicPageSize)
                .Select(r => new RuinCardViewModel
                {
                    RuinId = r.RuinId,
                    RuinName = r.RuinName,
                    City = r.City,
                    District = r.District,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    TicketPrice = r.TicketPrice,
                    OpeningHours = r.OpeningHours,
                    HistoricalPeriod = r.HistoricalPeriod,
                    IsFeatured = r.IsFeatured,
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category.CategoryName
                })
                .ToListAsync();

            var selectedCategoryName = categoryId.HasValue
                ? categoryItems.FirstOrDefault(c => c.Value == categoryId.Value.ToString())?.Text
                : null;

            var viewModel = new RuinListViewModel
            {
                Ruins = ruins,
                Categories = categoryItems,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                SelectedCategoryName = selectedCategoryName,
                TotalCount = totalCount,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    PageSize = PublicPageSize,
                    TotalItems = totalCount
                }
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await BuildRuinDetailViewModelAsync(id);

            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "The requested ruin could not be found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = viewModel.RuinName;
            ViewBag.CategoryName = viewModel.CategoryName;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int ruinId, [Bind(Prefix = "NewReview")] ReviewCreateViewModel newReview)
        {
            if (!await _context.Ruins.AsNoTracking().AnyAsync(r => r.RuinId == ruinId && r.IsActive))
            {
                TempData["ErrorMessage"] = "The requested ruin could not be found.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var viewModel = await BuildRuinDetailViewModelAsync(ruinId);
                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "The requested ruin could not be found.";
                    return RedirectToAction(nameof(Index));
                }

                viewModel.NewReview = newReview;
                ViewData["Title"] = viewModel.RuinName;
                ViewBag.CategoryName = viewModel.CategoryName;
                return View("Details", viewModel);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "Registered user";

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var review = new RuinReview
            {
                RuinId = ruinId,
                UserId = userId,
                UserEmail = userEmail,
                Rating = newReview.Rating,
                CommentText = newReview.CommentText.Trim(),
                CreatedAt = DateTime.Now
            };

            await _context.RuinReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your ruin review has been submitted.";
            return RedirectToAction(nameof(Details), new { id = ruinId });
        }

        private async Task<RuinDetailViewModel?> BuildRuinDetailViewModelAsync(int id)
        {
            var ruin = await _context.Ruins
                .AsNoTracking()
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.RuinId == id && r.IsActive);

            if (ruin == null)
            {
                return null;
            }

            var userId = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var isFavorite = !string.IsNullOrWhiteSpace(userId) &&
                await _context.Favorites
                    .AsNoTracking()
                    .AnyAsync(f => f.UserId == userId && f.RuinId == ruin.RuinId);

            var reviews = await _context.RuinReviews
                .AsNoTracking()
                .Where(r => r.RuinId == ruin.RuinId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewItemViewModel
                {
                    UserEmail = r.UserEmail,
                    Rating = r.Rating,
                    CommentText = r.CommentText,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new RuinDetailViewModel
            {
                RuinId = ruin.RuinId,
                RuinName = ruin.RuinName,
                City = ruin.City,
                District = ruin.District,
                Address = ruin.Address,
                Description = ruin.Description,
                ImageUrl = ruin.ImageUrl,
                TicketPrice = ruin.TicketPrice,
                OpeningHours = ruin.OpeningHours,
                HistoricalPeriod = ruin.HistoricalPeriod,
                GoogleMapsUrl = ruin.GoogleMapsUrl,
                IsFeatured = ruin.IsFeatured,
                IsFavorite = isFavorite,
                CategoryId = ruin.CategoryId,
                CategoryName = ruin.Category.CategoryName,
                Reviews = reviews,
                ReviewCount = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : null
            };
        }
    }
}
