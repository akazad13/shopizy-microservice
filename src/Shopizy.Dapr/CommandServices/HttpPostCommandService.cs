using Dapr.Client;

namespace Shopizy.Dapr.CommandServices;

public record HttpPostCommand<TBody>(string ServiceName, string UrlPath, TBody Body);

public class HttpPostCommandService<TBody>(DaprClient dapr) : ICommandService<HttpPostCommand<TBody>>
{
    private readonly DaprClient _dapr = dapr;

    public async Task ExecuteAsync(HttpPostCommand<TBody> q, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage req = _dapr.CreateInvokeMethodRequest(q.ServiceName, q.UrlPath, q.Body);
        await _dapr.InvokeMethodAsync(req, cancellationToken);
    }
}

