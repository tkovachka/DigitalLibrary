using LibraryApplication.Domain.Domain;
using LibraryApplication.Domain.DTO;
using LibraryApplication.Service.Interface;

namespace LibraryApplication.Service.API
{
    public class GoogleBookImporter
    {
        private readonly IBookService _bookService;
        private readonly IPublisherService _publisherService;
        private readonly IAuthorService _authorService;
        private readonly ICategoryService _categoryService;

        public GoogleBookImporter(IBookService bookService, IPublisherService publisherService, IAuthorService authorService, ICategoryService categoryService)
        {
            _bookService=bookService;
            _publisherService=publisherService;
            _authorService=authorService;
            _categoryService=categoryService;
        }

        // Import DTOs in bulk with caching to minimize DB hits
        public async Task<(int inserted, int skipped)> ImportAsync(IEnumerable<BookDTO> dtos, CancellationToken ct = default)
        {
            var dtoList = dtos.ToList();
            if (!dtoList.Any()) return (0, 0);

            // Preload existing data
            var existingBooks = _bookService.GetAll()
                .Select(b => new { b.Id, b.Isbn10, b.Isbn13 });

            var existingIsbn10 = new HashSet<string>(existingBooks.Where(b => !string.IsNullOrEmpty(b.Isbn10)).Select(b => b.Isbn10!), StringComparer.OrdinalIgnoreCase);
            var existingIsbn13 = new HashSet<string>(existingBooks.Where(b => !string.IsNullOrEmpty(b.Isbn13)).Select(b => b.Isbn13!), StringComparer.OrdinalIgnoreCase);

            var authors = _authorService.GetAll();
            var authorByName = authors.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

            var publishers = _publisherService.GetAll();
            var publisherByName = publishers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            var categories = _categoryService.GetAll();
            var categoryByName = categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

            var newAuthors = new List<Author>();
            var newPublishers = new List<Publisher>();
            var newCategories = new List<Category>();
            var toInsertBooks = new List<Book>();

            foreach (var dto in dtoList)
            {
                if (string.IsNullOrWhiteSpace(dto.Isbn10))
                    continue;

                if (existingIsbn10.Contains(dto.Isbn10) || (!string.IsNullOrWhiteSpace(dto.Isbn13) && existingIsbn13.Contains(dto.Isbn13)))
                    continue; // already exists

                var book = new Book
                {
                    Id = Guid.NewGuid(),
                    Isbn10 = dto.Isbn10,
                    Isbn13 = dto.Isbn13,
                    Title = dto.Title,
                    Subtitle = dto.Subtitle,
                    PageCount = dto.PageCount,
                    Language = dto.Language,
                    AverageRating = dto.AverageRating,
                    RatingsCount = dto.RatingsCount,
                    Description = dto.Description,
                    ThumbnailUrl = dto.ThumbnailUrl,
                    IsAvailable = true
                };

                // Publisher
                if (!string.IsNullOrWhiteSpace(dto.Publisher))
                {
                    if (!publisherByName.TryGetValue(dto.Publisher, out var pub))
                    {
                        pub = new Publisher
                        {
                            Id = Guid.NewGuid(),
                            Name = dto.Publisher
                        };
                        publisherByName.Add(pub.Name, pub);
                        newPublishers.Add(pub);
                    }
                    book.Publisher = pub;
                    book.PublisherId = pub.Id;
                }

                // Authors
                foreach (var authorName in dto.Authors.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(authorName)) continue;

                    if (!authorByName.TryGetValue(authorName, out var author))
                    {
                        author = new Author
                        {
                            Id = Guid.NewGuid(),
                            Name = authorName.Trim()
                        };
                        authorByName.Add(author.Name, author);
                        newAuthors.Add(author);
                    }
                    book.Authors.Add(author);
                }

                // Categories
                foreach (var catName in dto.Categories.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(catName)) continue;
                    if (!categoryByName.TryGetValue(catName, out var cat))
                    {
                        cat = new Category
                        {
                            Id = Guid.NewGuid(),
                            Name = catName.Trim()
                        };
                        categoryByName.Add(cat.Name, cat);
                        newCategories.Add(cat);
                    }
                    book.Categories.Add(cat);
                }

                if (dto.PublishedDate.HasValue)
                    book.PublishedDate = dto.PublishedDate;

                toInsertBooks.Add(book);
                existingIsbn10.Add(book.Isbn10);

                if (!string.IsNullOrWhiteSpace(book.Isbn13))
                    existingIsbn13.Add(book.Isbn13);
            }

            using var transaction = await _bookService.BeginTransactionAsync();
            try
            {
                if (newPublishers.Any()) _publisherService.InsertAll(newPublishers, saveChanges: false);
                if (newAuthors.Any()) _authorService.InsertAll(newAuthors, saveChanges: false);
                if (newCategories.Any()) _categoryService.InsertAll(newCategories, saveChanges: false);
                if (toInsertBooks.Any()) _bookService.InsertAll(toInsertBooks, saveChanges: true);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return (toInsertBooks.Count, dtoList.Count - toInsertBooks.Count);
        }


    }
}
