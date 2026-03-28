using FluentAssertions;
using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Implementation;
using LibraryApplication.Service.Interface;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryApplication.Tests
{
    public class LoanServiceTests
    {
        private readonly Mock<IRepository<Loan>> _loanRepoMock = new();
        private readonly Mock<IReservationService> _reservationServiceMock = new();
        private readonly Mock<IBookService> _bookServiceMock = new();
        private readonly Mock<ILogger<LoanService>> _loggerMock = new();
        private readonly LoanService _sut;

        public LoanServiceTests()
        {
            _sut = new LoanService(
                _loanRepoMock.Object,
                _reservationServiceMock.Object,
                _bookServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void LoanBook_WhenBookIsAvailable_CreatesLoan()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var userId = "user-123";
            var book = new Book { Id = bookId, Title = "Test Book", IsAvailable = true, Language = "en" };

            _loanRepoMock.Setup(r => r.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, Loan>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, bool>>>(),
                null, null))
                .Returns((Loan?)null); // no active loan

            _bookServiceMock.Setup(s => s.GetById(bookId)).Returns(book);
            _loanRepoMock.Setup(r => r.Insert(It.IsAny<Loan>()))
                .Returns<Loan>(l => l);
            _bookServiceMock.Setup(s => s.Update(It.IsAny<Book>()))
                .Returns<Book>(b => b);

            // Act
            var result = _sut.LoanBook(bookId, userId, null);

            // Assert
            result.Should().NotBeNull();
            result.BorrowedBookId.Should().Be(bookId);
            result.UserId.Should().Be(userId);
            result.DateReturned.Should().BeNull();
        }

        [Fact]
        public void LoanBook_WhenBookAlreadyLoaned_ThrowsInvalidOperationException()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var existingLoan = new Loan { Id = Guid.NewGuid(), BorrowedBookId = bookId, UserId = "other-user" };

            _loanRepoMock.Setup(r => r.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, Loan>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, bool>>>(),
                null, null))
                .Returns(existingLoan);

            // Act
            var act = () => _sut.LoanBook(bookId, "user-123", null);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*currently loaned*");
        }

        [Fact]
        public void LoanBook_WithEmptyUserId_ThrowsArgumentNullException()
        {
            // Act
            var act = () => _sut.LoanBook(Guid.NewGuid(), "", null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ReturnBook_WhenLoanBelongsToUser_SetsDateReturned()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var userId = "user-123";
            var loan = new Loan { Id = loanId, BorrowedBookId = bookId, UserId = userId };
            var book = new Book { Id = bookId, Title = "Test", Language = "en" };

            _loanRepoMock.Setup(r => r.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, Loan>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, bool>>>(),
                null, It.IsAny<Func<IQueryable<Loan>,
                    Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Loan, object>>>()))
                .Returns(loan);

            _loanRepoMock.Setup(r => r.Update(It.IsAny<Loan>())).Returns<Loan>(l => l);
            _bookServiceMock.Setup(s => s.GetById(bookId)).Returns(book);
            _bookServiceMock.Setup(s => s.Update(It.IsAny<Book>())).Returns<Book>(b => b);
            _reservationServiceMock.Setup(s => s.GetQueueForBook(bookId))
                .Returns(new List<Reservation>());

            // Act
            _sut.ReturnBook(loanId, userId);

            // Assert
            loan.DateReturned.Should().NotBeNull();
            loan.DateReturned.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
        }

        [Fact]
        public void ReturnBook_WhenLoanBelongsToDifferentUser_ThrowsException()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan { Id = loanId, UserId = "owner-user" };

            _loanRepoMock.Setup(r => r.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, Loan>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Loan, bool>>>(),
                null, It.IsAny<Func<IQueryable<Loan>,
                    Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Loan, object>>>()))
                .Returns(loan);

            // Act
            var act = () => _sut.ReturnBook(loanId, "different-user");

            // Assert
            act.Should().Throw<Exception>()
               .WithMessage("*not permitted*");
        }
    }
}