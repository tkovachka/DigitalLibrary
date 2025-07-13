using LibraryApplication.Domain.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Service.Interface
{
    public interface IAuthorService
    {
        Author? GetById(Guid id);
        List<Author> GetAll();
        Author Add(Author author); 
        Author Update(Author author);
        Author DeleteById(Guid id);
        bool Exists(string name);
        ICollection<Author> GetAllByIds(List<Guid> authorIds);
    }
}
