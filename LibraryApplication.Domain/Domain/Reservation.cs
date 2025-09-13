using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Domain.Domain
{
    public class Reservation : BaseEntity
    {
        public Guid BookId { get; set; }
        public virtual Book? Book { get; set; }
        public string? UserId { get; set; }
        public int QueuePosition { get; set; }
        public DateOnly DateRequested { get; set; }
        public DateOnly? ActivationDate { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        [NotMapped]
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    }
}
