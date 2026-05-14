namespace Heritage_of_Turkey.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
