using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Service.Implementation
{
    public class AuthorService : IAuthorService
    {
        private readonly IRepository<Author> _repository;

        public AuthorService(IRepository<Author> repository)
        {
            _repository=repository;
        }

        public Author Add(Author author)
        {
            return _repository.Insert(author);
        }

        public Author DeleteById(Guid id)
        {
            var author = GetById(id);
            if (author == null) throw new Exception("Author not found");
            return _repository.Delete(author);
        }

        public bool Exists(string name)
        {
            Author? author = _repository.Get(selector: x=>x, predicate:x=>x.Name.Equals(name));
            return author != null;
        }

        public List<Author> GetAll()
        {
            return _repository.GetAll(x => x).ToList();
        }

        public ICollection<Author> GetAllByIds(List<Guid> authorIds)
        {
            return _repository.GetAll(selector: x => x, predicate: x => authorIds.Contains(x.Id)).ToList();
        }

        public Author? GetById(Guid id)
        {
            return _repository.Get(selector: x=>x, predicate: x=>x.Id.Equals(id), include: x=>x.Include(z=>z.Books));
        }

        public void InsertAll(List<Author> authors, bool saveChanges = true)
        {
            _repository.InsertAll(authors, saveChanges);
        }

        public Author Update(Author author)
        {
            return _repository.Update(author);
        }
    }
}
