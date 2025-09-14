namespace LibraryApplication.Domain.DTO
{
    public class BookDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public List<string> Authors { get; set; } = new();
        public string? Publisher { get; set; }
        public string? PublishedDateRaw { get; set; }
        public DateOnly? PublishedDate { get; set; }
        public string? Isbn10 { get; set; }
        public string? Isbn13 { get; set; }
        public int? PageCount { get; set; }
        public string? Language { get; set; }
        public double? AverageRating { get; set; }
        public int? RatingsCount { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<string> Categories { get; set; } = new();
    }
}
