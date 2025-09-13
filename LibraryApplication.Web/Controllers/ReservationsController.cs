
using LibraryApplication.Domain.Domain;
using LibraryApplication.Domain.Identity;
using LibraryApplication.Repository.Data;
using LibraryApplication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryApplication.Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService=reservationService;
        }



        // GET: Reservations
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return View(_reservationService.GetReservationsByUser(userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reserve(Guid bookId)
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
                return RedirectToAction("Details", "Book", new { id = bookId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(Guid id)
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
                return RedirectToAction("Index");
            }
        }

    }
}
