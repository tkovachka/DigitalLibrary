using LibraryApplication.Service.API;
using LibraryApplication.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace LibraryApplication.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IPublisherService _publisherService;
        private readonly ICategoryService _categoryService;
        private readonly ILoanService _loanService;
        private readonly IReservationService _reservationService;

        private readonly GoogleBooksClient _googleBooksClient;
        private readonly GoogleBookImporter _googleBookImporter;

        public BooksController(IBookService bookService, IAuthorService authorService, IPublisherService publisherService, ICategoryService categoryService, ILoanService loanService, IReservationService reservationService, GoogleBooksClient googleBooksClient, GoogleBookImporter googleBookImporter)
        {
            _bookService=bookService;
            _authorService=authorService;
            _publisherService=publisherService;
            _categoryService=categoryService;
            _loanService=loanService;
            _reservationService=reservationService;
            _googleBooksClient=googleBooksClient;
            _googleBookImporter=googleBookImporter;
        }

        // GET: Books
        public IActionResult Index()
        {
            return View(_bookService.GetAll());
        }

        // GET: Books/Details/5
        public IActionResult Details(Guid id)
        {
            var book = _bookService.GetById(id);
            if (book == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ViewData["LoanId"] = null;
            ViewData["ReservationId"] = null;
            ViewData["UsersLoan"] = false;
            ViewData["UsersReservation"] = false;
            ViewData["ActiveReservation"] = false;

            if (!string.IsNullOrEmpty(userId))
            {
                var loan = _loanService.GetActiveLoanForBook(book.Id);

                if (loan != null)
                {
                    if (string.Equals(loan.UserId, userId, StringComparison.OrdinalIgnoreCase))
                    {
                        // User currently has the loan
                        ViewData["LoanId"] = loan.Id;
                        ViewData["UsersLoan"] = true;
                    }
                    else
                    {
                        // Someone else has the loan, check if this user has a reservation
                        var reservation = _reservationService
                            .GetReservationsByUser(userId)
                            .FirstOrDefault(x => x.BookId == book.Id);

                        if (reservation != null)
                        {
                            ViewData["UsersReservation"] = true;
                            ViewData["ReservationId"] = reservation.Id;
                            if (reservation.IsActive)
                                ViewData["ActiveReservation"] = true;
                        }
                    }
                }
                else
                {
                    // No loan exists, book is available, but user might still have an active reservation
                    var reservation = _reservationService
                        .GetReservationsByUser(userId)
                        .FirstOrDefault(x => x.BookId == book.Id);

                    if (reservation != null)
                    {
                        ViewData["UsersReservation"] = true;
                        ViewData["ReservationId"] = reservation.Id;
                        if (reservation.IsActive)
                            ViewData["ActiveReservation"] = true;
                    }
                }
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["Authors"] = new SelectList(_authorService.GetAll(), "Id", "Name");
            ViewData["Publishers"] = new SelectList(_publisherService.GetAll(), "Id", "Name");
            ViewData["Categories"] = new SelectList(_categoryService.GetAll(), "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Isbn10,Isbn13,Title,Subtitle,PublishedDate,PageCount,Language,AverageRating,RatingsCount,Description,ThumbnailUrl,PublisherId,AuthorIds,CategoryIds")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.Id = Guid.NewGuid();
                _bookService.Add(book, book.AuthorIds, book.CategoryIds);
                return RedirectToAction(nameof(Index));
            }
            ViewData["Authors"] = new SelectList(_authorService.GetAll(), "Id", "Name");
            ViewData["Publishers"] = new SelectList(_publisherService.GetAll(), "Id", "Name");
            ViewData["Categories"] = new SelectList(_categoryService.GetAll(), "Id", "Name");
            return View(book);
        }

        // GET: Books/Edit/5
        public IActionResult Edit(Guid id)
        {
            var book = _bookService.GetById(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["Authors"] = new SelectList(_authorService.GetAll(), "Id", "Name");
            ViewData["Publishers"] = new SelectList(_publisherService.GetAll(), "Id", "Name");
            ViewData["Categories"] = new SelectList(_categoryService.GetAll(), "Id", "Name");
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Isbn10,Isbn13,Title,Subtitle,PublishedDate,PageCount,Language,AverageRating,RatingsCount,Description,ThumbnailUrl,PublisherId,AuthorIds,CategoryIds,Id")] Book book)
        {
            if (ModelState.IsValid)
            {
                _bookService.Update(book);
                return RedirectToAction(nameof(Index));
            }
            ViewData["Authors"] = new SelectList(_authorService.GetAll(), "Id", "Name");
            ViewData["Publishers"] = new SelectList(_publisherService.GetAll(), "Id", "Name");
            ViewData["Categories"] = new SelectList(_categoryService.GetAll(), "Id", "Name");
            return View(book);
        }

        // GET: Books/Delete/5
        public IActionResult Delete(Guid id)
        {
            var book =_bookService.GetById(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _bookService.DeleteById(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromGoogle(string query, int count = 10, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return BadRequest("query required");
            if (count < 1) return BadRequest("count must be > 0");
            // Limit total count, to avoid runaway imports; each loop call will use max 40 per request
            if (count > 40) count = 40;

            try
            {
                var books = await _googleBooksClient.SearchAndParseAsync(query, count, ct);
                var (inserted, skipped) = await _googleBookImporter.ImportAsync(books, ct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            //return Ok(new { requested = count, found = books.Count, inserted, skipped });
            return RedirectToAction(nameof(Index));
        }
    }
}
