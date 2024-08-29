using Dapr.Client;

namespace Shopizy.Dapr.CommandServices;

public record HttpPatchCommand<TBody>(string ServiceName, string UrlPath, TBody Body);
public class HttpPatchCommandService<TBody>(DaprClient dapr) : ICommandService<HttpPutCommand<TBody>>
{
    private readonly DaprClient _dapr = dapr;

    public async Task ExecuteAsync(HttpPutCommand<TBody> q, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage req = _dapr.CreateInvokeMethodRequest(HttpMethod.Patch, q.ServiceName, q.UrlPath, null, q.Body);
        await _dapr.InvokeMethodAsync(req, cancellationToken);
    }
}