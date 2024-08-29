namespace Shopizy.Dapr.QueryServices;

public interface IQueryService<TQuery, TResult>
{
    public Task<TResult> QueryAsync(TQuery query);
}
