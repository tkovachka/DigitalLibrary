using LibraryApplication.Domain.Domain;

namespace LibraryApplication.Service.Interface
{
    public interface IPublisherService
    {
        Publisher? GetById(Guid id);
        List<Publisher> GetAll();
        Publisher Add(Publisher publisher);
        void InsertAll(List<Publisher> publishers, bool saveChanges = true);
        Publisher Update(Publisher publisher);
        Publisher DeleteById(Guid id);
        bool Exists(string name);
        int DeleteAll(bool saveChanges = true);

    }
}
