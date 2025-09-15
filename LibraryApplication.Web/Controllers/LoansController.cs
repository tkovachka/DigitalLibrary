using LibraryApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApplication.Web.Controllers
{
    [Authorize(Roles = "User")]
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IReservationService _reservationService;
        private readonly IBookService _bookService;

        public LoansController(ILoanService loanService, IReservationService reservationService, IBookService bookService)
        {
            _loanService=loanService;
            _reservationService=reservationService;
            _bookService=bookService;
        }

        // GET: Loans for current logged in user
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var activeLoans = _loanService.GetLoansByUser(userId);
            var loansHistory = _loanService.GetLoansHistoryByUser(userId);
            ViewData["History"] = loansHistory;
            //todo fix the active loans loading (database is ok, service is the problem somewhere)
            return View(activeLoans);
        }

        public IActionResult Loan(Guid bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound("Log in to make a loan");
            var model = _bookService.GetById(bookId);
            if (model == null) return NotFound("Book not found");
            var reservation = _reservationService.GetReservationsByUser(userId).FirstOrDefault(x => x.BookId.Equals(bookId));
            ViewData["ReservationId"] = reservation?.Id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoanConfirm(Guid bookId, Guid? reservationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound("Log in to confirm your loan");
            try
            {
                _loanService.LoanBook(bookId, userId, reservationId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //todo show error to user on screen
                TempData["Error"] = ex.Message;
                return NotFound(ex.Message);
            }
        }

        public IActionResult Details(Guid id)
        {
            var model = _loanService.GetById(id);
            if (model == null)
                return NotFound("Loan not found");
            return View(model);
        }

        public IActionResult Return(Guid id)
        {
            var model = _loanService.GetById(id);
            if (model == null)
                return NotFound("Loan not found");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReturnConfirmation(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                _loanService.ReturnBook(id, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //todo show user error message
                TempData["Error"] = ex.Message;
                return NotFound(ex.Message);
            }
        }
    }
}
