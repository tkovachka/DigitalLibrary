namespace LibraryApplication.Service.Settings
{
    public class GoogleBooksSettings
    {
        public const string SectionName = "GoogleBooks";
        public string? ApiKey { get; set; }
        public int DefaultMaxResults { get; set; } = 10;
    }
}
