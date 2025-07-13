using LibraryApplication.Domain.Domain;
using Microsoft.AspNetCore.Identity;

namespace LibraryApplication.Domain.Identity
{
    public class LibraryApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public DateOnly? CreationDate { get; set; }
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
