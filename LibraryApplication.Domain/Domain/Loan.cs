using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Domain.Domain
{
    public class Loan : BaseEntity
    {
        public Guid BookId { get; set; }
        public virtual Book? BorrowedBook { get; set; }
        public string? UserId { get; set; }
        public DateOnly DateBorrowed { get; set; }
        public DateOnly DateReturned { get; set; }

    }
}
