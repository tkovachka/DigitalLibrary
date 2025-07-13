using LibraryApplication.Domain.Domain;

namespace LibraryApplication.Service.Interface
{
    public interface ICategoryService
    {
        List<Category> GetAll();
        Category? GetById(Guid Id);
        Category Add(Category category);
        Category Update(Category category);
        Category DeleteById(Guid Id);
        bool Exists(string name);
        ICollection<Category> GetAllByIds(List<Guid> categoryIds);
    }
}
