using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var r = Get(reservationId);
            if (r == null) throw new Exception("Reservation not found");
            if (!string.Equals(r.UserId, userId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Not allowed to cancel this reservation.");
            
            Delete(r);

            //renumber queue positions
            var queue = GetQueueForBook(r.BookId);
            int idx = 1;
            foreach (var res in queue) {
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
            return _reservationRepository.GetAll(selector: x => x, predicate: x => x.UserId == userId).ToList();
        }

        public Reservation ReserveBook(Guid bookId, string userId)
        {
            if(string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException("UserId not found");
            var activeLoan = _loanRepository.Get(selector: x => x,
              predicate: x => x.BorrowedBookId.Equals(bookId) && x.DateReturned == null);
            if (activeLoan != null) throw new InvalidOperationException("Book is available, reserving not required. Borrow book now!");

            var queue = GetQueueForBook(bookId);
            if (queue.Any(r => string.Equals(r.UserId, userId, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("You already have a reservation for this book.");

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
            throw new NotImplementedException();
        }

        public Reservation Insert(Reservation reservation)
        {
            return _reservationRepository.Insert(reservation);
        }

        public Reservation? Get(Guid reservationId)
        {
            return _reservationRepository.Get(selector: x => x, predicate: x => x.Id == reservationId);
        }

        public Reservation? Delete(Reservation reservation)
        {
            return _reservationRepository.Delete(reservation);
        }

        public Reservation? Update(Reservation reservation)
        {
            return _reservationRepository.Update(reservation);
        }
    }
}
