using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Heritage_of_Turkey.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Category")]
    public class CategoryAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("", Name = "AdminCategories")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Museums)
                .Include(c => c.Ruins)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryAdminItemViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    MuseumCount = c.Museums.Count,
                    RuinCount = c.Ruins.Count
                })
                .ToListAsync();

            var viewModel = new CategoryAdminListViewModel
            {
                Categories = categories,
                TotalCount = categories.Count,
                ActiveCount = categories.Count(c => c.IsActive),
                InactiveCount = categories.Count(c => !c.IsActive)
            };

            ViewData["Title"] = "Categories";

            return View("~/Views/Admin/Category/Index.cshtml", viewModel);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Category";

            return View("~/Views/Admin/Category/Create.cshtml", new CategoryAdminFormViewModel());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryAdminFormViewModel model)
        {
            ViewData["Title"] = "Create Category";

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Category/Create.cshtml", model);
            }

            var categoryName = model.CategoryName.Trim();

            var categoryExists = await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.CategoryName == categoryName);

            if (categoryExists)
            {
                ModelState.AddModelError(nameof(model.CategoryName), "A category with this name already exists.");
                return View("~/Views/Admin/Category/Create.cshtml", model);
            }

            var category = new Category
            {
                CategoryName = categoryName,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category created successfully.";

            return RedirectToRoute("AdminCategories");
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Museums)
                .Include(c => c.Ruins)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category could not be found.";
                return RedirectToRoute("AdminCategories");
            }

            var viewModel = new CategoryAdminDetailViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                MuseumCount = category.Museums.Count,
                RuinCount = category.Ruins.Count
            };

            ViewData["Title"] = category.CategoryName;

            return View("~/Views/Admin/Category/Details.cshtml", viewModel);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category could not be found.";
                return RedirectToRoute("AdminCategories");
            }

            var viewModel = new CategoryAdminFormViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive
            };

            ViewData["Title"] = "Edit Category";

            return View("~/Views/Admin/Category/Edit.cshtml", viewModel);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryAdminFormViewModel model)
        {
            ViewData["Title"] = "Edit Category";

            if (id != model.CategoryId)
            {
                TempData["ErrorMessage"] = "Invalid category request.";
                return RedirectToRoute("AdminCategories");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Category/Edit.cshtml", model);
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category could not be found.";
                return RedirectToRoute("AdminCategories");
            }

            var categoryName = model.CategoryName.Trim();

            var duplicateExists = await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.CategoryId != id && c.CategoryName == categoryName);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(model.CategoryName), "A category with this name already exists.");
                return View("~/Views/Admin/Category/Edit.cshtml", model);
            }

            category.CategoryName = categoryName;
            category.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            category.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category updated successfully.";

            return RedirectToRoute("AdminCategories");
        }

        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Museums)
                .Include(c => c.Ruins)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category could not be found.";
                return RedirectToRoute("AdminCategories");
            }

            var viewModel = new CategoryAdminDetailViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                MuseumCount = category.Museums.Count,
                RuinCount = category.Ruins.Count
            };

            ViewData["Title"] = "Delete Category";

            return View("~/Views/Admin/Category/Delete.cshtml", viewModel);
        }

        [HttpPost("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Museums)
                .Include(c => c.Ruins)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category could not be found.";
                return RedirectToRoute("AdminCategories");
            }

            if (category.Museums.Any() || category.Ruins.Any())
            {
                TempData["ErrorMessage"] = "This category cannot be deleted because it has related museums or ruins.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category deleted successfully.";

            return RedirectToRoute("AdminCategories");
        }
    }
}
