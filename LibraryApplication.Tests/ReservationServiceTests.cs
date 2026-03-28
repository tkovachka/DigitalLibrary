using FluentAssertions;
using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Implementation;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApplication.Tests
{
    public class ReservationServiceTests
    {
        private readonly Mock<IRepository<Reservation>> _reservationRepoMock = new();
        private readonly Mock<IRepository<Loan>> _loanRepoMock = new();
        private readonly Mock<ILogger<ReservationService>> _loggerMock = new();
        private readonly ReservationService _sut;

        public ReservationServiceTests()
        {
            _sut = new ReservationService(
                _reservationRepoMock.Object,
                _loanRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void ReserveBook_WhenBookIsAvailable_ThrowsInvalidOperationException()
        {
            // Book is available means no active loan
            _loanRepoMock.Setup(r => r.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, Loan>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, bool>>>(),
                null, null))
                .Returns((Loan?)null);

            var act = () => _sut.ReserveBook(Guid.NewGuid(), "user-123");

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*available*");
        }

        [Fact]
        public void ReserveBook_WhenUserAlreadyHasReservation_ThrowsInvalidOperationException()
        {
            var bookId = Guid.NewGuid();
            var userId = "user-123";

            _loanRepoMock.Setup(r => r.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, Loan>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, bool>>>(),
                null, null))
                .Returns(new Loan()); // active loan exists

            _reservationRepoMock.Setup(r => r.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, Reservation>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(),
                null, null, false))
                .Returns(new List<Reservation>
                {
                    new Reservation { BookId = bookId, UserId = userId, QueuePosition = 1 }
                });

            var act = () => _sut.ReserveBook(bookId, userId);

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*already have a reservation*");
        }

        [Fact]
        public void ReserveBook_WhenValid_AssignsCorrectQueuePosition()
        {
            var bookId = Guid.NewGuid();

            _loanRepoMock.Setup(r => r.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, Loan>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, bool>>>(),
                null, null))
                .Returns(new Loan());

            _reservationRepoMock.Setup(r => r.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, Reservation>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(),
                null, null, false))
                .Returns(new List<Reservation>
                {
                    new Reservation { QueuePosition = 1 },
                    new Reservation { QueuePosition = 2 }
                });

            _reservationRepoMock.Setup(r => r.Insert(It.IsAny<Reservation>()))
                .Returns<Reservation>(r => r);

            var result = _sut.ReserveBook(bookId, "user-123");

            result.QueuePosition.Should().Be(3); // third in queue
        }
    }
}