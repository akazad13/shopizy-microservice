using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.Aggregates.Categories.ValueObjects;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Catelog.API.Persistence.Categories.Specifications;
using Shopizy.Domain.Models.Specifications;

namespace Shopizy.Catelog.API.Persistence.Categories;

public class CategoryRepository(AppDbContext dbContext) : ICategoryRepository
{
    private readonly AppDbContext _dbContext = dbContext;
    public Task<bool> GetCategoryByNameAsync(string name)
    {
        return _dbContext.Categories.AnyAsync(category => category.Name == name);
    }

    public Task<Category?> GetCategoryByIdAsync(CategoryId id)
    {
        return ApplySpec(new CategoryByIdSpec(id)).FirstOrDefaultAsync();
    }
    public Task<List<Category>> GetCategoriesAsync()
    {
        return _dbContext.Categories.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        await _dbContext.Categories.AddAsync(category);
    }

    public void Update(Category category)
    {
        _dbContext.Update(category);
    }

    public void Remove(Category category)
    {
        _dbContext.Remove(category);
    }

    public Task<int> Commit(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
    private IQueryable<Category> ApplySpec(Specification<Category> spec)
    {
        return SpecificationEvaluator.GetQuery(_dbContext.Categories, spec);
    }
}