using System.Linq.Expressions;

namespace Shopizy.Domain.Models.Specifications;

public abstract class Specification<TEntity> where TEntity : class
{
    protected Specification() { }
    protected Specification(Expression<Func<TEntity, bool>>? criteria)
    {
        Criteria = criteria;
    }
    public Expression<Func<TEntity, bool>>? Criteria { get; }
    public IList<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = [];
    public Expression<Func<TEntity, object>>? OrderByExpression { get; private set; }
    public Expression<Func<TEntity, object>>? OrderByDecendingExpression { get; private set; }

    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression) =>
        IncludeExpressions.Add(includeExpression);

    protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression) =>
        OrderByExpression = orderByExpression;

    protected void AddOrderByDecending(Expression<Func<TEntity, object>> orderByDecendingExpression) =>
        OrderByDecendingExpression = orderByDecendingExpression;
}

