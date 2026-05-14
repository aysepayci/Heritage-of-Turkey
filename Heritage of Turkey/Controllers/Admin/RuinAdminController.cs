using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Admin;
using Heritage_of_Turkey.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Ruin")]
    public class RuinAdminController : Controller
    {
        private const int AdminPageSize = 10;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RuinAdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("", Name = "AdminRuins")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            page = Math.Max(page, 1);

            var ruinsQuery = _context.Ruins
                .AsNoTracking()
                .Include(r => r.Category);

            var totalCount = await ruinsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)AdminPageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var ruins = await ruinsQuery
                .OrderByDescending(r => r.CreatedDate)
                .Skip((page - 1) * AdminPageSize)
                .Take(AdminPageSize)
                .Select(r => new RuinAdminItemViewModel
                {
                    RuinId = r.RuinId,
                    RuinName = r.RuinName,
                    City = r.City,
                    District = r.District,
                    ImageUrl = r.ImageUrl,
                    TicketPrice = r.TicketPrice,
                    HistoricalPeriod = r.HistoricalPeriod,
                    IsFeatured = r.IsFeatured,
                    IsActive = r.IsActive,
                    CreatedDate = r.CreatedDate,
                    CategoryName = r.Category.CategoryName
                })
                .ToListAsync();

            var viewModel = new RuinAdminListViewModel
            {
                Ruins = ruins,
                TotalCount = totalCount,
                ActiveCount = await _context.Ruins.AsNoTracking().CountAsync(r => r.IsActive),
                FeaturedCount = await _context.Ruins.AsNoTracking().CountAsync(r => r.IsFeatured),
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    PageSize = AdminPageSize,
                    TotalItems = totalCount
                }
            };

            ViewData["Title"] = "Ruins";

            return View("~/Views/Admin/Ruin/Index.cshtml", viewModel);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Create Ruin";

            var viewModel = new RuinAdminFormViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };

            return View("~/Views/Admin/Ruin/Create.cshtml", viewModel);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RuinAdminFormViewModel model)
        {
            ViewData["Title"] = "Create Ruin";

            if (!await CategoryExistsAsync(model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Ruin/Create.cshtml", model);
            }

            string? imageUrl;

            try
            {
                imageUrl = await ResolveImageUrlAsync(model.ImageFile, model.ImageUrl, "ruins");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Ruin/Create.cshtml", model);
            }

            var ruin = new Ruin
            {
                RuinName = model.RuinName.Trim(),
                City = model.City.Trim(),
                District = string.IsNullOrWhiteSpace(model.District) ? null : model.District.Trim(),
                Address = model.Address.Trim(),
                Description = model.Description.Trim(),
                ImageUrl = imageUrl,
                TicketPrice = model.TicketPrice,
                OpeningHours = string.IsNullOrWhiteSpace(model.OpeningHours) ? null : model.OpeningHours.Trim(),
                HistoricalPeriod = string.IsNullOrWhiteSpace(model.HistoricalPeriod) ? null : model.HistoricalPeriod.Trim(),
                GoogleMapsUrl = string.IsNullOrWhiteSpace(model.GoogleMapsUrl) ? null : model.GoogleMapsUrl.Trim(),
                IsFeatured = model.IsFeatured,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CategoryId = model.CategoryId
            };

            await _context.Ruins.AddAsync(ruin);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruin created successfully.";

            return RedirectToRoute("AdminRuins");
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var ruin = await _context.Ruins
                .AsNoTracking()
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.RuinId == id);

            if (ruin == null)
            {
                TempData["ErrorMessage"] = "Ruin could not be found.";
                return RedirectToRoute("AdminRuins");
            }

            ViewData["Title"] = ruin.RuinName;

            return View("~/Views/Admin/Ruin/Details.cshtml", ToDetailViewModel(ruin));
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var ruin = await _context.Ruins
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RuinId == id);

            if (ruin == null)
            {
                TempData["ErrorMessage"] = "Ruin could not be found.";
                return RedirectToRoute("AdminRuins");
            }

            ViewData["Title"] = "Edit Ruin";

            var viewModel = new RuinAdminFormViewModel
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
                IsActive = ruin.IsActive,
                CategoryId = ruin.CategoryId,
                Categories = await GetCategorySelectListAsync(ruin.CategoryId)
            };

            return View("~/Views/Admin/Ruin/Edit.cshtml", viewModel);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RuinAdminFormViewModel model)
        {
            ViewData["Title"] = "Edit Ruin";

            if (id != model.RuinId)
            {
                TempData["ErrorMessage"] = "Invalid ruin request.";
                return RedirectToRoute("AdminRuins");
            }

            if (!await CategoryExistsAsync(model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Ruin/Edit.cshtml", model);
            }

            var ruin = await _context.Ruins
                .FirstOrDefaultAsync(r => r.RuinId == id);

            if (ruin == null)
            {
                TempData["ErrorMessage"] = "Ruin could not be found.";
                return RedirectToRoute("AdminRuins");
            }

            string? imageUrl;

            try
            {
                imageUrl = await ResolveImageUrlAsync(model.ImageFile, model.ImageUrl, "ruins", ruin.ImageUrl);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Ruin/Edit.cshtml", model);
            }

            ruin.RuinName = model.RuinName.Trim();
            ruin.City = model.City.Trim();
            ruin.District = string.IsNullOrWhiteSpace(model.District) ? null : model.District.Trim();
            ruin.Address = model.Address.Trim();
            ruin.Description = model.Description.Trim();
            ruin.ImageUrl = imageUrl;
            ruin.TicketPrice = model.TicketPrice;
            ruin.OpeningHours = string.IsNullOrWhiteSpace(model.OpeningHours) ? null : model.OpeningHours.Trim();
            ruin.HistoricalPeriod = string.IsNullOrWhiteSpace(model.HistoricalPeriod) ? null : model.HistoricalPeriod.Trim();
            ruin.GoogleMapsUrl = string.IsNullOrWhiteSpace(model.GoogleMapsUrl) ? null : model.GoogleMapsUrl.Trim();
            ruin.IsFeatured = model.IsFeatured;
            ruin.IsActive = model.IsActive;
            ruin.UpdatedDate = DateTime.Now;
            ruin.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruin updated successfully.";

            return RedirectToRoute("AdminRuins");
        }

        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ruin = await _context.Ruins
                .AsNoTracking()
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.RuinId == id);

            if (ruin == null)
            {
                TempData["ErrorMessage"] = "Ruin could not be found.";
                return RedirectToRoute("AdminRuins");
            }

            ViewData["Title"] = "Delete Ruin";

            return View("~/Views/Admin/Ruin/Delete.cshtml", ToDetailViewModel(ruin));
        }

        [HttpPost("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ruin = await _context.Ruins
                .FirstOrDefaultAsync(r => r.RuinId == id);

            if (ruin == null)
            {
                TempData["ErrorMessage"] = "Ruin could not be found.";
                return RedirectToRoute("AdminRuins");
            }

            _context.Ruins.Remove(ruin);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruin deleted successfully.";

            return RedirectToRoute("AdminRuins");
        }

        private async Task<IList<SelectListItem>> GetCategorySelectListAsync(int? selectedCategoryId = null)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName,
                    Selected = selectedCategoryId.HasValue && c.CategoryId == selectedCategoryId.Value
                })
                .ToListAsync();
        }

        private async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.CategoryId == categoryId && c.IsActive);
        }

        private async Task<string?> ResolveImageUrlAsync(IFormFile? imageFile, string? manualImageUrl, string folderName, string? existingImageUrl = null)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                return await SaveImageAsync(imageFile, folderName);
            }

            if (!string.IsNullOrWhiteSpace(manualImageUrl))
            {
                return manualImageUrl.Trim();
            }

            return existingImageUrl;
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile, string folderName)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only .jpg, .jpeg, .png, and .webp image files are allowed.");
            }

            var webRootPath = _webHostEnvironment.WebRootPath
                            ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            var uploadsRoot = Path.Combine(webRootPath, "images", folderName); Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/images/{folderName}/{fileName}";
        }

        private static RuinAdminDetailViewModel ToDetailViewModel(Ruin ruin)
        {
            return new RuinAdminDetailViewModel
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
                IsActive = ruin.IsActive,
                CreatedDate = ruin.CreatedDate,
                UpdatedDate = ruin.UpdatedDate,
                CategoryName = ruin.Category.CategoryName
            };
        }
        [HttpGet("Import")]
        public IActionResult Import()
        {
            ViewData["Title"] = "Import Ruins from CSV";
            return View("~/Views/Admin/Ruin/Import.cshtml");
        }

        [HttpPost("ValidateAndPreview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateAndPreview(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return Json(new { success = false, message = "Lütfen bir CSV dosyası seçin" });
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Lütfen .csv dosyası yükleyin" });
            }

            try
            {
                var service = new CsvImportService(_context);
                using (var stream = csvFile.OpenReadStream())
                {
                    var validationResults = await service.ValidateMuseumCsvAsync(stream);

                    var successCount = validationResults.Count(r => r.Status == "Success");
                    var warningCount = validationResults.Count(r => r.Status == "Warning");
                    var errorCount = validationResults.Count(r => r.Status == "Error");

                    HttpContext.Session.SetObjectAsJson("MuseumImportResults", validationResults);

                    return Json(new
                    {
                        success = true,
                        successCount = successCount,
                        warningCount = warningCount,
                        errorCount = errorCount,
                        results = validationResults.Select(r => new
                        {
                            r.RowNumber,
                            r.Status,
                            r.ItemName,
                            r.City,
                            r.Message
                        }).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpPost("ConfirmAndImport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAndImport()
        {
            try
            {
                var validationResults = HttpContext.Session.GetObjectFromJson<List<CsvImportResult>>("MuseumImportResults");

                if (validationResults == null || !validationResults.Any())
                {
                    return Json(new { success = false, message = "Önce CSV dosyasını validate edin" });
                }

                var service = new CsvImportService(_context);
                var (successCount, skipCount, errorResults) = await service.ImportMuseumsAsync(validationResults);

                HttpContext.Session.Remove("MuseumImportResults");

                return Json(new
                {
                    success = true,
                    successCount = successCount,
                    skipCount = skipCount,
                    message = $"✅ {successCount} müze başarıyla eklendi{(skipCount > 0 ? $", {skipCount} atlandı" : "")}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Import hatası: {ex.Message}" });
            }
        }
    }
}
