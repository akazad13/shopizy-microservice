using Dapr.Client;

namespace Shopizy.Dapr.QueryServices;

public class HttpGetQueryHandler<TResult>(DaprClient dapr) : IQueryService<HttpGetQuery, TResult>
{
    private readonly DaprClient _dapr = dapr;

    public Task<TResult> QueryAsync(HttpGetQuery q)
    {
        return _dapr.InvokeMethodAsync<TResult>(HttpMethod.Get, q.ServiceName, q.UrlPath);
    }
}
