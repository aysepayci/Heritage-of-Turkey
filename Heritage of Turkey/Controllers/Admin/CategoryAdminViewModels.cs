using System.ComponentModel.DataAnnotations;

namespace Heritage_of_Turkey.ViewModels.Admin
{
    public class CategoryAdminListViewModel
    {
        public IList<CategoryAdminItemViewModel> Categories { get; set; } = new List<CategoryAdminItemViewModel>();

        public int TotalCount { get; set; }

        public int ActiveCount { get; set; }

        public int InactiveCount { get; set; }
    }

    public class CategoryAdminItemViewModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public int MuseumCount { get; set; }

        public int RuinCount { get; set; }
    }

    public class CategoryAdminFormViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class CategoryAdminDetailViewModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public int MuseumCount { get; set; }

        public int RuinCount { get; set; }
    }
}