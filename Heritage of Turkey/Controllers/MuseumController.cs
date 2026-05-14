using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Museum;
using Heritage_of_Turkey.ViewModels.Reviews;
using Heritage_of_Turkey.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Heritage_of_Turkey.Controllers
{
    public class MuseumController : Controller
    {
        private const int PublicPageSize = 6;
        private readonly ApplicationDbContext _context;

        public MuseumController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int page = 1)
        {
            ViewData["Title"] = "Museums";
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

            var museumsQuery = _context.Museums
                .AsNoTracking()
                .Include(m => m.Category)
                .Where(m => m.IsActive);

            if (categoryId.HasValue)
            {
                museumsQuery = museumsQuery.Where(m => m.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var keyword = searchTerm.Trim();
                var searchPattern = $"%{keyword}%";

                museumsQuery = museumsQuery.Where(m =>
                    EF.Functions.Like(m.MuseumName, searchPattern) ||
                    EF.Functions.Like(m.City, searchPattern) ||
                    (m.District != null && EF.Functions.Like(m.District, searchPattern)) ||
                    EF.Functions.Like(m.Description, searchPattern) ||
                    EF.Functions.Like(m.Category.CategoryName, searchPattern));
            }

            var totalCount = await museumsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PublicPageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var museums = await museumsQuery
                .OrderByDescending(m => m.IsFeatured)
                .ThenBy(m => m.MuseumName)
                .Skip((page - 1) * PublicPageSize)
                .Take(PublicPageSize)
                .Select(m => new MuseumCardViewModel
                {
                    MuseumId = m.MuseumId,
                    MuseumName = m.MuseumName,
                    City = m.City,
                    District = m.District,
                    Description = m.Description,
                    ImageUrl = m.ImageUrl,
                    TicketPrice = m.TicketPrice,
                    OpeningHours = m.OpeningHours,
                    IsFeatured = m.IsFeatured,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category.CategoryName
                })
                .ToListAsync();

            var selectedCategoryName = categoryId.HasValue
                ? categoryItems.FirstOrDefault(c => c.Value == categoryId.Value.ToString())?.Text
                : null;

            var viewModel = new MuseumListViewModel
            {
                Museums = museums,
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
            var viewModel = await BuildMuseumDetailViewModelAsync(id);

            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "The requested museum could not be found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = viewModel.MuseumName;
            ViewBag.CategoryName = viewModel.CategoryName;

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int museumId, [Bind(Prefix = "NewReview")] ReviewCreateViewModel newReview)
        {
            if (!await _context.Museums.AsNoTracking().AnyAsync(m => m.MuseumId == museumId && m.IsActive))
            {
                TempData["ErrorMessage"] = "The requested museum could not be found.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var viewModel = await BuildMuseumDetailViewModelAsync(museumId);
                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "The requested museum could not be found.";
                    return RedirectToAction(nameof(Index));
                }

                viewModel.NewReview = newReview;
                ViewData["Title"] = viewModel.MuseumName;
                ViewBag.CategoryName = viewModel.CategoryName;
                return View("Details", viewModel);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "Registered user";

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var review = new MuseumReview
            {
                MuseumId = museumId,
                UserId = userId,
                UserEmail = userEmail,
                Rating = newReview.Rating,
                CommentText = newReview.CommentText.Trim(),
                CreatedAt = DateTime.Now
            };

            await _context.MuseumReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your museum review has been submitted.";
            return RedirectToAction(nameof(Details), new { id = museumId });
        }

        private async Task<MuseumDetailViewModel?> BuildMuseumDetailViewModelAsync(int id)
        {
            var museum = await _context.Museums
                .AsNoTracking()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MuseumId == id && m.IsActive);

            if (museum == null)
            {
                return null;
            }

            var userId = User.Identity?.IsAuthenticated == true
               ? User.FindFirstValue(ClaimTypes.NameIdentifier)
               : null;

            var isFavorite = !string.IsNullOrWhiteSpace(userId) &&
                await _context.Favorites
                    .AsNoTracking()
                    .AnyAsync(f => f.UserId == userId && f.MuseumId == museum.MuseumId);

            var reviews = await _context.MuseumReviews
                .AsNoTracking()
                .Where(r => r.MuseumId == museum.MuseumId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewItemViewModel
                {
                    UserEmail = r.UserEmail,
                    Rating = r.Rating,
                    CommentText = r.CommentText,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new MuseumDetailViewModel
            {
                MuseumId = museum.MuseumId,
                MuseumName = museum.MuseumName,
                City = museum.City,
                District = museum.District,
                Address = museum.Address,
                Description = museum.Description,
                ImageUrl = museum.ImageUrl,
                TicketPrice = museum.TicketPrice,
                OpeningHours = museum.OpeningHours,
                PhoneNumber = museum.PhoneNumber,
                Email = museum.Email,
                Website = museum.Website,
                GoogleMapsUrl = museum.GoogleMapsUrl,
                IsFeatured = museum.IsFeatured,
                IsFavorite = isFavorite,
                CategoryId = museum.CategoryId,
                CategoryName = museum.Category.CategoryName,
                Reviews = reviews,
                ReviewCount = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : null
            };
        }
    }
}
