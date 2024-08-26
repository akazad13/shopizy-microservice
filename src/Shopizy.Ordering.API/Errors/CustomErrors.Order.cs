using ErrorOr;

namespace Shopizy.Ordering.API.Errors;

public static partial class CustomErrors
{
    public static class Order
    {
        public static Error OrderNotFound =>
            Error.NotFound(code: "Order.OrderNotFound", description: "Order is not found.");
        public static Error OrderNotCreated =>
            Error.Failure(code: "Order.OrderNotCreated", description: "Failed to create Order.");
        public static Error OrderNotDeleted =>
            Error.Failure(code: "Order.OrderNotDeleted", description: "Failed to delete Order.");

        public static Error OrderNotCancelled =>
            Error.Failure(code: "Order.OrderNotCancelled", description: "Failed to cancel Order.");

        public static Error OrderNotUpdated =>
            Error.Failure(code: "Order.OrderNotUpdated", description: "Failed to update Order.");
    }
}