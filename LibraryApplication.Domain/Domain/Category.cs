using System.ComponentModel.DataAnnotations;

namespace LibraryApplication.Domain.Domain
{
    public class Category : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    }
}

