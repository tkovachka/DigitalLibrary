using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
