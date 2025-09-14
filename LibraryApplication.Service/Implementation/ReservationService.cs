using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryApplication.Service.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly IRepository<Reservation> _reservationRepository;
        private readonly IRepository<Loan> _loanRepository;

        public ReservationService(IRepository<Reservation> reservationRepository, IRepository<Loan> loanRepository)
        {
            _reservationRepository=reservationRepository;
            _loanRepository=loanRepository;
        }

        public void CancelReservation(Guid reservationId, string userId)
        {
            var r = GetById(reservationId);
            if (r == null) throw new Exception("Reservation not found");
            if (!string.Equals(r.UserId, userId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Not allowed to cancel this reservation.");

            bool isActive = r.IsActive;
            DeleteById(r.Id);

            //if reservation is active, activate the next in queue  deleting
            if(isActive)
            {
                var queue = GetQueueForBook(r.BookId);

                var next = queue.FirstOrDefault(x => x.Id != r.Id); 
                if (next != null)
                {
                    next.IsActive = true;
                    Update(next);
                }
            }

            //renumber queue positions
            var queueUpdated = GetQueueForBook(r.BookId);
            int idx = 1;
            foreach (var res in queueUpdated) {
                res.QueuePosition = idx++;
                Update(res);
            }            

        }

        public List<Reservation> GetQueueForBook(Guid bookId)
        {
            List<Reservation> queue = _reservationRepository.GetAll(selector: x => x, predicate: x => x.BookId == bookId)
                                           .OrderBy(r => r.QueuePosition == 0 ? int.MaxValue : r.QueuePosition)
                                           .ThenBy(r => r.DateRequested)
                                           .ToList();
            if (queue.IsNullOrEmpty()) queue = new List<Reservation>();
            return queue;
        }

        public List<Reservation> GetReservationsByUser(string userId)
        {
            return _reservationRepository.GetAll(selector: x => x, predicate: x => x.UserId.Equals(userId), include: x=> x.Include(z=>z.Book)).ToList();
        }

        public Reservation ReserveBook(Guid bookId, string userId)
        {
            if(string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException("UserId not found");
            var activeLoan = _loanRepository.Get(selector: x => x,
              predicate: x => x.BorrowedBookId.Equals(bookId) && x.DateReturned == null);
            if (activeLoan == null) throw new InvalidOperationException("Book is available, reserving not required. Borrow book now!");

            var queue = GetQueueForBook(bookId);
            if (queue.Any(r => string.Equals(r.UserId, userId, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("You already have a reservation for this book. See status at My Reservations.");

            var position = queue.Count + 1;
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                BookId = bookId,
                UserId = userId,
                QueuePosition = position,
                DateRequested = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = false
            };

            Insert(reservation);
            return reservation;
        }

        public void ActivateReservation(Guid reservationId, string userId)
        {
            var reservation = GetById(reservationId);
            if (reservation == null)
                throw new Exception("Reservation not found");

            if (!string.Equals(reservation.UserId, userId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Reservation does not belong to this user");

            if (reservation.IsActive)
                throw new InvalidOperationException("Loan from this reservation is already active");

            reservation.IsActive = true;
            _reservationRepository.Update(reservation);

        }

        public void FulfillReservation(Guid reservationId, string userId)
        {
            var reservation = GetById(reservationId);
            if (reservation == null)
                throw new Exception("Reservation not found");

            if (!string.Equals(reservation.UserId, userId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Reservation does not belong to this user");

            var bookId = reservation.BookId;

            DeleteById(reservation.Id);
            
            var queue = GetQueueForBook(bookId);

            foreach (var r in queue)
            {
                r.QueuePosition--;
                Update(r);
            }
        }

        public Reservation Insert(Reservation reservation)
        {
            return _reservationRepository.Insert(reservation);
        }

        public Reservation? GetById(Guid reservationId)
        {
            return _reservationRepository.Get(selector: x => x, predicate: x => x.Id == reservationId, include: x=>x.Include(z=>z.Book).ThenInclude(y => y.Authors));
        }

        public Reservation? DeleteById(Guid reservationId)
        {
            var r = GetById(reservationId);
            return _reservationRepository.Delete(r);
        }

        public Reservation? Update(Reservation reservation)
        {
            return _reservationRepository.Update(reservation);
        }
    }
}
