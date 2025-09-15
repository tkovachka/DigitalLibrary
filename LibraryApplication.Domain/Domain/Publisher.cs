using System.ComponentModel.DataAnnotations;

namespace LibraryApplication.Domain.Domain
{
    public class Publisher : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
