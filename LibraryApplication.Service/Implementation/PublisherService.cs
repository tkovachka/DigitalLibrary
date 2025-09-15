using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryApplication.Service.Implementation
{
    public class PublisherService : IPublisherService
    {
        private readonly IRepository<Publisher> _repository;

        public PublisherService(IRepository<Publisher> repository)
        {
            _repository=repository;
        }

        public Publisher Add(Publisher publisher)
        {
            return _repository.Insert(publisher);
        }

        public int DeleteAll(bool saveChanges = true)
        {
            return _repository.DeleteAll(saveChanges);
        }

        public Publisher DeleteById(Guid id)
        {
            Publisher? publisher = GetById(id);
            if (publisher == null) throw new Exception("Publisher not found");
            return _repository.Delete(publisher);
        }

        public bool Exists(string name)
        {
            Publisher? publisher = _repository.Get(selector: x => x, predicate: x => x.Name.Equals(name));
            return publisher != null;
        }

        public List<Publisher> GetAll()
        {
            return _repository.GetAll(selector: x => x, 
                include:x=>x.Include(z=>z.Books)).ToList();
        }

        public Publisher? GetById(Guid id)
        {
            return _repository.Get(selector: x => x, predicate: x => x.Id.Equals(id),
                include:x=>x.Include(z=>z.Books));
        }

        public void InsertAll(List<Publisher> publishers, bool saveChanges = true)
        {
            _repository.InsertAll(publishers, saveChanges);
        }

        public Publisher Update(Publisher publisher)
        {
            return _repository.Update(publisher);
        }
    }
}
