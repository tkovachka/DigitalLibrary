using LibraryApplication.Domain.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Book : BaseEntity
{
    public string? Isbn10 { get; set; } = string.Empty;
    public string? Isbn13 { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public DateOnly? PublishedDate { get; set; }
    public int? PageCount { get; set; }
    [Required]
    public string? Language { get; set; }    // ISO 639‑1 code (e.g., "en")
    [Range(0, 5)]
    public double? AverageRating { get; set; }
    public int? RatingsCount { get; set; }
    public string? Description { get; set; }
    [Url]
    public string? ThumbnailUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Guid PublisherId { get; set; }
    public virtual Publisher? Publisher { get; set; }
    [NotMapped]        
    public List<Guid> AuthorIds { get; set; } = new();   
    [NotMapped]
    public List<Guid> CategoryIds { get; set; } = new();   
    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}