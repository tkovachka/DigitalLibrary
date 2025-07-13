using LibraryApplication.Domain.Domain;

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
        List<Book> GetAllBooksByPublisherId(Guid publisherId);
        List<Book> GetAllBooksByCategoryId(Guid categoryId);
        List<Book> GetAllBooksByAuthorId(Guid authorId);

    }
}
