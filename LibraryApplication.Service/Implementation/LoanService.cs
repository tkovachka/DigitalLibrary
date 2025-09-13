using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LibraryApplication.Service.Implementation
{
    public class LoanService : ILoanService
    {
        private readonly IRepository<Loan> _loanRepository;
        private readonly IReservationService _reservationService;
        private readonly IBookService _bookService;

        public LoanService(IRepository<Loan> loanRepository, IReservationService reservationService, IBookService bookService)
        {
            _loanRepository=loanRepository;
            _reservationService=reservationService;
            _bookService=bookService;
        }

        public Loan? GetById(Guid loanId)
        {
            return _loanRepository.Get(selector: x=>x, predicate: x=>x.Id.Equals(loanId), include: x=>x.Include(z=>z.BorrowedBook));
        }

        public Loan? GetActiveLoanForBook(Guid bookId)
        {
            return _loanRepository.Get(selector: x => x,
                predicate: x => x.BorrowedBookId.Equals(bookId) && x.DateReturned == null);
        }

        public List<Loan> GetLoansByUser(string userId)
        {
            return _loanRepository.GetAll(selector: x => x,
                        predicate: x => (x.UserId == userId) && !x.DateReturned.HasValue,
                        include: q => q.Include(l => l.BorrowedBook),
                        asNoTracking: true)
                        .ToList();
        }

        public List<Loan> GetLoansHistoryByUser(string userId)
        {
            return _loanRepository.GetAll(selector: x => x,
                        predicate: x => (x.UserId == userId) && x.DateReturned.HasValue,
                        include: q => q.Include(l => l.BorrowedBook),
                        asNoTracking: true)
                        .ToList();
        }

        public Loan LoanBook(Guid bookId, string userId, Reservation fromReservation)
        {
            if(string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException("UserId required", nameof(userId));

            var activeLoan = GetActiveLoanForBook(bookId);
            // TODO: show message to user here and stop further action
            if (activeLoan != null) throw new InvalidOperationException("Book is currently loaned. Make a reservation or wait your turn.");
            var book = _bookService.GetById(bookId);
            if (book == null) throw new Exception("Book not found");

            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                BorrowedBookId = bookId,
                UserId = userId,
                DateBorrowed = DateOnly.FromDateTime(DateTime.UtcNow),
                DateReturned = null
            };
            _loanRepository.Insert(loan);
            book.IsAvailable = false;
            _bookService.Update(book);
            if(fromReservation != null)
            {
                _reservationService.FulfillReservation(fromReservation.Id, userId);
            }
            return loan;
        }

        public Loan Update(Loan loan)
        {
            return _loanRepository.Update(loan);
        }

        public void ReturnBook(Guid loanId, string userId)
        {
            var loan = GetById(loanId);
            if (loan == null) throw new Exception("Loan not found");
            if (!string.Equals(loan.UserId, userId, StringComparison.OrdinalIgnoreCase)) throw new Exception("User not permitted to return book");
            
            loan.DateReturned = DateOnly.FromDateTime(DateTime.UtcNow);
            Update(loan);
            var book = _bookService.GetById(loan.BorrowedBookId);
            if (book == null) throw new Exception("Book not found");

            //activate next reservation for the book
            var queue = _reservationService.GetQueueForBook(loan.BorrowedBookId);
            var next = queue.FirstOrDefault(r => !r.IsActive);
            if (next != null)
            {
                _reservationService.ActivateReservation(next.Id, next.UserId);
            }
            else 
            {
               //book is only available after there are no more reservations in the queue
                book.IsAvailable = true;            
                _bookService.Update(book);
            }
        }
    }
}
