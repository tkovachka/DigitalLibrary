using LibraryApplication.Domain.Domain;

namespace LibraryApplication.Service.Interface
{
    public interface IReservationService
    {
        Reservation? GetById(Guid reservationId);
        Reservation? DeleteById(Guid reservationId);
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
