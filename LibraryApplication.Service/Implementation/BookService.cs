using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryApplication.Service.Implementation
{
    public class BookService : IBookService
    {
        private readonly IRepository<Book> _repository;
        private readonly ICategoryService _categoryService;
        private readonly IAuthorService _authorService;
        private readonly IPublisherService _publisherService;

        public BookService(IRepository<Book> repository, ICategoryService categoryService, IAuthorService authorService, IPublisherService publisherService)
        {
            _repository=repository;
            _categoryService=categoryService;
            _authorService=authorService;
            _publisherService=publisherService;
        }

        public Book Add(Book book)
        {
            book.IsAvailable = true;
            return _repository.Insert(book);
        }

        public Book Add(Book book, List<Guid> authorIds, List<Guid> categoryIds)
        {
            book.Authors = _authorService.GetAllByIds(authorIds);
            book.Categories = _categoryService.GetAllByIds(categoryIds);
            return _repository.Insert(book);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return _repository.BeginTransactionAsync();
        }

        public int DeleteAll(bool saveChanges = true)
        {
            return _repository.DeleteAll(saveChanges);
        }

        public Book DeleteById(Guid Id)
        {
            Book? book = GetById(Id);
            if (book == null) throw new Exception("Book not found");
            return _repository.Delete(book);
        }

        public List<Book> GetAll()
        {
            return _repository.GetAll(selector: x => x,
                include: x => x.Include(z => z.Authors).Include(z => z.Publisher).Include(z => z.Categories)).ToList();
        }

        public List<Book> GetAllBooksByAuthorId(Guid authorId)
        {
            var author = _authorService.GetById(authorId);
            return _repository.GetAll(selector: x => x, predicate: x => x.Authors.Contains(author),
                include: x => x.Include(z => z.Authors).Include(z => z.Publisher).Include(z => z.Categories)).ToList();
        }

        public List<Book> GetAllBooksByCategoryId(Guid categoryId)
        {
            var category = _categoryService.GetById(categoryId);
            return _repository.GetAll(selector: x => x, predicate: x => x.Categories.Contains(category),
                include: x => x.Include(z => z.Authors).Include(z => z.Publisher).Include(z => z.Categories)).ToList();
        }

        public List<Book> GetAllBooksByPublisherId(Guid publisherId)
        {
            var publisher = _publisherService.GetById(publisherId);
            return _repository.GetAll(selector: x => x, predicate: x => x.Publisher != null && x.Publisher.Equals(publisher),
                include: x => x.Include(z => z.Authors).Include(z => z.Publisher).Include(z => z.Categories)).ToList();
        }

        public Book? GetById(Guid Id)
        {
            return _repository.Get(selector: x => x,
                predicate: x => x.Id.Equals(Id),
                include: x => x.Include(z => z.Authors).Include(z => z.Publisher).Include(z => z.Categories));
        }

        public void InsertAll(List<Book> books, bool saveChanges = true)
        {
            _repository.InsertAll(books, saveChanges);
        }

        public Book Update(Book book)
        {
            return _repository.Update(book);
        }
    }
}
