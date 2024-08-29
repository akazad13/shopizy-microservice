using Dapr.Client;
using Shopizy.Dapr.Constants;

namespace Shopizy.Dapr.QueryServices.Products;

public record IsProductExistQuery(Guid ProductId);

public class IsProductExistQueryService(DaprClient dapr) : IQueryService<IsProductExistQuery, bool>
{
    private readonly DaprClient _dapr = dapr;

    public Task<bool> QueryAsync(IsProductExistQuery query)
    {
        return _dapr.InvokeMethodAsync<bool>(HttpMethod.Get, ServiceInvocation.SHOPIZY_SERVICE_CATELOG, $"api/v1.0/products/{query.ProductId}/isexist");
    }

}
