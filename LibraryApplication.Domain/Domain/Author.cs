using System.ComponentModel.DataAnnotations;

namespace LibraryApplication.Domain.Domain
{
    public class Author : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
