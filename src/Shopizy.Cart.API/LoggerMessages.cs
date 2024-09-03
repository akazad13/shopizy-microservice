namespace Shopizy.Cart.API;

public static partial class LoggerMessages
{

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Error,
        Message = "An error occurred while initialising the database.")]
    public static partial void DatabaseInitializationError(
        this ILogger logger,
        Exception ex);
}
