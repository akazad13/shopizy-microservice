using Microsoft.EntityFrameworkCore;
using Shopizy.Domain.Models.Specifications;

namespace Shopizy.Catelog.API.Persistence;

public static class SpecificationEvaluator
{
    public static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> inputQueryable,
        Specification<TEntity> specification
    ) where TEntity : class
    {
        IQueryable<TEntity> queryable = inputQueryable;

        if (specification.Criteria is not null)
            queryable = queryable.Where(specification.Criteria);

        _ = specification.IncludeExpressions.Aggregate(
            queryable,
            (current, includeExpression) =>
            {
                return current.Include(includeExpression);
            }
        );

        if (specification.OrderByExpression is not null)
            queryable = queryable.OrderBy(specification.OrderByExpression);
        else if (specification.OrderByDecendingExpression is not null)
            queryable = queryable.OrderByDescending(specification.OrderByDecendingExpression);

        return queryable;
    }
}
