using System.ComponentModel.DataAnnotations;

namespace LibraryApplication.Domain.Domain
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
