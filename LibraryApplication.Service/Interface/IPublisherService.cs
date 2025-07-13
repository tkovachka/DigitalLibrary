using LibraryApplication.Domain.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Service.Interface
{
    public interface IPublisherService
    {
        Publisher? GetById(Guid id);
        List<Publisher> GetAll();
        Publisher Add(Publisher publisher);
        Publisher Update(Publisher publisher);
        Publisher DeleteById(Guid id);
        bool Exists(string name);
    }
}
