using LibraryApplication.Domain.Domain;

namespace LibraryApplication.Service.Interface
{
    public interface IAuthorService
    {
        Author? GetById(Guid id);
        List<Author> GetAll();
        Author Add(Author author);
        void InsertAll(List<Author> authors, bool saveChanges = true);
        Author Update(Author author);
        Author DeleteById(Guid id);
        bool Exists(string name);
        ICollection<Author> GetAllByIds(List<Guid> authorIds);
        int DeleteAll(bool saveChanges = true);

    }
}
