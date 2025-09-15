using LibraryApplication.Domain.Domain;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace LibraryApplication.Service.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _repository;

        public CategoryService(IRepository<Category> repository)
        {
            _repository=repository;
        }

        public Category Add(Category category)
        {
            return _repository.Insert(category);
        }

        public int DeleteAll(bool saveChanges = true)
        {
            return _repository.DeleteAll(saveChanges);
        }

        public Category DeleteById(Guid Id)
        {
            Category? category = GetById(Id);
            if (category == null) throw new Exception("Category not found");
            return _repository.Delete(category);
        }

        public bool Exists(string name)
        {
            Category? category = _repository.Get(selector: x => x, predicate: x => x.Name.Equals(name));
            return category != null;
        }

        public List<Category> GetAll()
        {
            return _repository.GetAll(selector:x => x, include:x=>x.Include(z=>z.Books)).ToList();
        }

        public ICollection<Category> GetAllByIds(List<Guid> categoryIds)
        {
            return _repository.GetAll(selector: x => x, predicate: x => categoryIds.Contains(x.Id), include:x=>x.Include(z=>z.Books)).ToList();
        }

        public Category? GetById(Guid Id)
        {
            return _repository.Get(selector: x => x, predicate: x => x.Id.Equals(Id), include: x=>x.Include(z=>z.Books));
        }

        public void InsertAll(List<Category> categories, bool saveChanges = true)
        {
            _repository.InsertAll(categories, saveChanges);
        }

        public Category Update(Category category)
        {
            return _repository.Update(category);
        }
    }
}
