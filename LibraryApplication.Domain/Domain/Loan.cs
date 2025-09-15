using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryApplication.Domain.Domain
{
    public class Loan : BaseEntity
    {
        public Guid BorrowedBookId { get; set; }
        public virtual Book? BorrowedBook { get; set; }
        public string? UserId { get; set; }
        public DateOnly DateBorrowed { get; set; }
        public DateOnly? DateReturned { get; set; }
        [NotMapped]
        public bool IsActive => DateReturned == null;

    }
}
