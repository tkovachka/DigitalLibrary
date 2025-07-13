using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Data;
using LibraryApplication.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApplication.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IPublisherService _publisherService;
        private readonly ICategoryService _categoryService;

        public BooksController(IBookService bookService, IAuthorService authorService, IPublisherService publisherService, ICategoryService categoryService)
        {
            _bookService=bookService;
            _authorService=authorService;
            _publisherService=publisherService;
            _categoryService=categoryService;
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
            {
                return NotFound();
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

    }
}
