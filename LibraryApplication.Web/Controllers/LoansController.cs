using LibraryApplication.Domain.Domain;
using LibraryApplication.Domain.Identity;
using LibraryApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApplication.Web.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService=loanService;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Loan(Guid bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                _loanService.LoanBook(bookId, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //todo show error to user on screen
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Books", new { id = bookId });
            }
        }

        public IActionResult Details(Guid id)
        {
            var model = _loanService.GetById(id);
            if (model == null)
                throw new Exception("Loan not found");
            return View(model);
        }

        public IActionResult Return(Guid id) {
            var model = _loanService.GetById(id);
            if (model == null)
                throw new Exception("Loan not found");
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
                return RedirectToAction("Index");
            }
        }
    }
}
