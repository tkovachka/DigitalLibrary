namespace LibraryApplication.Domain.DTO
{
    public class IndustryIdentifier
    {
        public string? Type { get; set; } // "ISBN_10" or "ISBN_13"
        public string? Identifier { get; set; }
    }
}
