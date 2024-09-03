using Ardalis.GuardClauses;
using MediatR;
using Shopizy.Domain.Models.Base;

namespace Shopizy.Catelog.API.Persistence;

public class EventualConsistencyMiddleware(RequestDelegate next)
{
    public readonly RequestDelegate _next = next;
    public const string DomainEventsKey = "DomainEventsKey";

    public async Task InvokeAsync(HttpContext context, IPublisher publisher, CatelogDbContext dbContext)
    {
        _ = Guard.Against.Null(context);
        _ = Guard.Against.Null(publisher);
        _ = Guard.Against.Null(dbContext);

        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();
        context.Response.OnCompleted(async () =>
        {
            try
            {
                if (context.Items.TryGetValue(DomainEventsKey, out object? value) && value is Queue<IDomainEvent> domainEvent)
                {
                    while (domainEvent.TryDequeue(out IDomainEvent? nextEvent))
                    {
                        await publisher.Publish(nextEvent);
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Ignore for now
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        });

        await _next(context);
    }
}
