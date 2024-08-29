namespace Shopizy.Dapr.CommandServices;

public interface ICommandService<T>
{
    public Task ExecuteAsync(T command, CancellationToken cancellationToken = default);
}
