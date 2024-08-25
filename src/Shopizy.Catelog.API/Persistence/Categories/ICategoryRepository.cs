using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Categories;

namespace Shopizy.Catelog.API.Persistence.Categories;

public interface ICategoryRepository
{
    Task<bool> GetCategoryByNameAsync(string name);
    Task<Category?> GetCategoryByIdAsync(CategoryId id);
    Task<List<Category>> GetCategoriesAsync();
    Task AddAsync(Category category);
    void Update(Category category);
    void Remove(Category category);
    Task<int> Commit(CancellationToken cancellationToken);
}