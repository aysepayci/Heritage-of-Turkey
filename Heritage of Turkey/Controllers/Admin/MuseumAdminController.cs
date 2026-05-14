using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Admin;
using Heritage_of_Turkey.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment
using Microsoft.AspNetCore.Http;  // For IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Museum")]
    public class MuseumAdminController : Controller
    {
        private const int AdminPageSize = 10;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MuseumAdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("", Name = "AdminMuseums")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int page = 1)
        {
            page = Math.Max(page, 1);

            var museumsQuery = _context.Museums
                .AsNoTracking()
                .Include(m => m.Category);

            var totalCount = await museumsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)AdminPageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var museums = await museumsQuery
                .OrderByDescending(m => m.CreatedDate)
                .Skip((page - 1) * AdminPageSize)
                .Take(AdminPageSize)
                .Select(m => new MuseumAdminItemViewModel
                {
                    MuseumId = m.MuseumId,
                    MuseumName = m.MuseumName,
                    City = m.City,
                    District = m.District,
                    ImageUrl = m.ImageUrl,
                    TicketPrice = m.TicketPrice,
                    IsFeatured = m.IsFeatured,
                    IsActive = m.IsActive,
                    CreatedDate = m.CreatedDate,
                    CategoryName = m.Category.CategoryName
                })
                .ToListAsync();

            var viewModel = new MuseumAdminListViewModel
            {
                Museums = museums,
                TotalCount = totalCount,
                ActiveCount = await _context.Museums.AsNoTracking().CountAsync(m => m.IsActive),
                FeaturedCount = await _context.Museums.AsNoTracking().CountAsync(m => m.IsFeatured),
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    PageSize = AdminPageSize,
                    TotalItems = totalCount
                }
            };

            ViewData["Title"] = "Museums";

            return View("~/Views/Admin/Museum/Index.cshtml", viewModel);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Create Museum";

            var viewModel = new MuseumAdminFormViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };

            return View("~/Views/Admin/Museum/Create.cshtml", viewModel);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MuseumAdminFormViewModel model)
        {
            ViewData["Title"] = "Create Museum";

            if (!await CategoryExistsAsync(model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Museum/Create.cshtml", model);
            }

            string? imageUrl;

            try
            {
                imageUrl = await ResolveImageUrlAsync(model.ImageFile, model.ImageUrl, "museums");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Museum/Create.cshtml", model);
            }

            var museum = new Museum
            {
                MuseumName = model.MuseumName.Trim(),
                City = model.City.Trim(),
                District = string.IsNullOrWhiteSpace(model.District) ? null : model.District.Trim(),
                Address = model.Address.Trim(),
                Description = model.Description.Trim(),
                ImageUrl = imageUrl,
                TicketPrice = model.TicketPrice,
                OpeningHours = string.IsNullOrWhiteSpace(model.OpeningHours) ? null : model.OpeningHours.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim(),
                GoogleMapsUrl = string.IsNullOrWhiteSpace(model.GoogleMapsUrl) ? null : model.GoogleMapsUrl.Trim(),
                IsFeatured = model.IsFeatured,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                CategoryId = model.CategoryId
            };

            await _context.Museums.AddAsync(museum);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Museum created successfully.";

            return RedirectToRoute("AdminMuseums");
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var museum = await _context.Museums
                .AsNoTracking()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MuseumId == id);

            if (museum == null)
            {
                TempData["ErrorMessage"] = "Museum could not be found.";
                return RedirectToRoute("AdminMuseums");
            }

            ViewData["Title"] = museum.MuseumName;

            return View("~/Views/Admin/Museum/Details.cshtml", ToDetailViewModel(museum));
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var museum = await _context.Museums
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MuseumId == id);

            if (museum == null)
            {
                TempData["ErrorMessage"] = "Museum could not be found.";
                return RedirectToRoute("AdminMuseums");
            }

            ViewData["Title"] = "Edit Museum";

            var viewModel = new MuseumAdminFormViewModel
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
                IsActive = museum.IsActive,
                CategoryId = museum.CategoryId,
                Categories = await GetCategorySelectListAsync(museum.CategoryId)
            };

            return View("~/Views/Admin/Museum/Edit.cshtml", viewModel);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MuseumAdminFormViewModel model)
        {
            ViewData["Title"] = "Edit Museum";

            if (id != model.MuseumId)
            {
                TempData["ErrorMessage"] = "Invalid museum request.";
                return RedirectToRoute("AdminMuseums");
            }

            if (!await CategoryExistsAsync(model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a valid category.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Museum/Edit.cshtml", model);
            }

            var museum = await _context.Museums
                .FirstOrDefaultAsync(m => m.MuseumId == id);

            if (museum == null)
            {
                TempData["ErrorMessage"] = "Museum could not be found.";
                return RedirectToRoute("AdminMuseums");
            }

            string? imageUrl;

            try
            {
                imageUrl = await ResolveImageUrlAsync(model.ImageFile, model.ImageUrl, "museums", museum.ImageUrl);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                model.Categories = await GetCategorySelectListAsync(model.CategoryId);
                return View("~/Views/Admin/Museum/Edit.cshtml", model);
            }

            museum.MuseumName = model.MuseumName.Trim();
            museum.City = model.City.Trim();
            museum.District = string.IsNullOrWhiteSpace(model.District) ? null : model.District.Trim();
            museum.Address = model.Address.Trim();
            museum.Description = model.Description.Trim();
            museum.ImageUrl = imageUrl;
            museum.TicketPrice = model.TicketPrice;
            museum.OpeningHours = string.IsNullOrWhiteSpace(model.OpeningHours) ? null : model.OpeningHours.Trim();
            museum.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
            museum.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            museum.Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim();
            museum.GoogleMapsUrl = string.IsNullOrWhiteSpace(model.GoogleMapsUrl) ? null : model.GoogleMapsUrl.Trim();
            museum.IsFeatured = model.IsFeatured;
            museum.IsActive = model.IsActive;
            museum.UpdatedDate = DateTime.Now;
            museum.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Museum updated successfully.";

            return RedirectToRoute("AdminMuseums");
        }

        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var museum = await _context.Museums
                .AsNoTracking()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MuseumId == id);

            if (museum == null)
            {
                TempData["ErrorMessage"] = "Museum could not be found.";
                return RedirectToRoute("AdminMuseums");
            }

            ViewData["Title"] = "Delete Museum";

            return View("~/Views/Admin/Museum/Delete.cshtml", ToDetailViewModel(museum));
        }

        [HttpPost("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var museum = await _context.Museums
                .FirstOrDefaultAsync(m => m.MuseumId == id);

            if (museum == null)
            {
                TempData["ErrorMessage"] = "Museum could not be found.";
                return RedirectToRoute("AdminMuseums");
            }

            _context.Museums.Remove(museum);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Museum deleted successfully.";

            return RedirectToRoute("AdminMuseums");
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

        private static MuseumAdminDetailViewModel ToDetailViewModel(Museum museum)
        {
            return new MuseumAdminDetailViewModel
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
                IsActive = museum.IsActive,
                CreatedDate = museum.CreatedDate,
                UpdatedDate = museum.UpdatedDate,
                CategoryName = museum.Category.CategoryName
            };
        }
        [HttpGet("Import")]
        public IActionResult Import()
        {
            ViewData["Title"] = "Import Museums from CSV";
            return View("~/Views/Admin/Museum/Import.cshtml");
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
