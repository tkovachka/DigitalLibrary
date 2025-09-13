using LibraryApplication.Domain.Domain;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Service.Interface
{
    public interface IReservationService
    {
        Reservation? GetById(Guid reservationId);
        Reservation? Delete(Reservation reservation);
        Reservation? Update(Reservation reservation);
        Reservation Insert(Reservation reservation);
        Reservation ReserveBook(Guid bookId, string userId); 
        List<Reservation> GetReservationsByUser(string userId);
        void CancelReservation(Guid reservationId, string userId);
        List<Reservation> GetQueueForBook(Guid bookId);
        void ActivateReservation(Guid reservationId, string userId);
        void FulfillReservation(Guid reservationId, string userId);
    }
}
