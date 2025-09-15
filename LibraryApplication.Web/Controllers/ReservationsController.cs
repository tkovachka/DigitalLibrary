using LibraryApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApplication.Web.Controllers
{
    [Authorize(Roles = "User")]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IBookService _bookService;

        public ReservationsController(IReservationService reservationService, IBookService bookService)
        {
            _reservationService=reservationService;
            _bookService=bookService;
        }


        // GET: Reservations
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return View(_reservationService.GetReservationsByUser(userId));
        }

        public IActionResult Details(Guid id)
        {
            var model = _reservationService.GetById(id);
            if (model == null) return NotFound("Reservation not found");
            return View(model);
        }

        public IActionResult Reserve(Guid bookId)
        {
            var model = _bookService.GetById(bookId);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReserveConfirm(Guid bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                _reservationService.ReserveBook(bookId, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //todo show error to user
                TempData["Error"] = ex.Message;
                return NotFound(ex.Message);
            }
        }

        public IActionResult Cancel(Guid id)
        {
            var model = _reservationService.GetById(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelConfirm(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                _reservationService.CancelReservation(id, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //todo show error to user
                TempData["Error"] = ex.Message;
                return NotFound(ex.Message);
            }
        }

    }
}
