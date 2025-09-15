using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryApplication.Service.Interface
{
    public interface IBookService
    {
        List<Book> GetAll();
        Book? GetById(Guid Id);
        Book Update(Book book);
        Book DeleteById(Guid Id);
        Book Add(Book book);
        Book Add(Book book, List<Guid> authorIds, List<Guid> categoryIds);
        void InsertAll(List<Book> books, bool saveChanges = true);
        int DeleteAll(bool saveChanges = true);
        List<Book> GetAllBooksByPublisherId(Guid publisherId);
        List<Book> GetAllBooksByCategoryId(Guid categoryId);
        List<Book> GetAllBooksByAuthorId(Guid authorId);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
