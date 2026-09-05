using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace Shopizy.SharedKernel.Middleware.Idempotency;

public sealed record CachedResponse(int StatusCode, string ContentType, string Body);

public interface IIdempotencyStore
{
    Task<CachedResponse?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, CachedResponse response, TimeSpan timeToLive, CancellationToken cancellationToken = default);
}

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private sealed record CacheEntry(CachedResponse Response, DateTime ExpiresAtUtc);
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public Task<CachedResponse?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAtUtc > DateTime.UtcNow)
            {
                return Task.FromResult<CachedResponse?>(entry.Response);
            }
            _cache.TryRemove(key, out _);
        }

        return Task.FromResult<CachedResponse?>(null);
    }

    public Task SetAsync(string key, CachedResponse response, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        _cache[key] = new CacheEntry(response, DateTime.UtcNow.Add(timeToLive));
        return Task.CompletedTask;
    }
}

public sealed class IdempotencyMiddleware
{
    public const string HeaderName = "Idempotency-Key";
    private readonly RequestDelegate _next;
    private readonly IIdempotencyStore _store;

    public IdempotencyMiddleware(RequestDelegate _next, IIdempotencyStore store)
    {
        this._next = _next;
        _store = store;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var keyHeader) ||
            string.IsNullOrWhiteSpace(keyHeader))
        {
            await _next(context);
            return;
        }

        var key = keyHeader.ToString();
        var cached = await _store.GetAsync(key, context.RequestAborted);
        if (cached is not null)
        {
            context.Response.StatusCode = cached.StatusCode;
            context.Response.ContentType = cached.ContentType;
            context.Response.Headers["X-Cache-Lookup"] = "HIT";
            await context.Response.WriteAsync(cached.Body, context.RequestAborted);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync(context.RequestAborted);
            memoryStream.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode is >= 200 and < 300)
            {
                var cachedResponse = new CachedResponse(
                    context.Response.StatusCode,
                    context.Response.ContentType ?? "application/json",
                    responseBody);

                await _store.SetAsync(key, cachedResponse, TimeSpan.FromSeconds(60), context.RequestAborted);
            }

            await memoryStream.CopyToAsync(originalBodyStream, context.RequestAborted);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
