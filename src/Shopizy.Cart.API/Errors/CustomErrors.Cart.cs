using ErrorOr;

namespace Shopizy.Cart.API.Errors;

public static partial class CustomErrors
{
    public static class Cart
    {
        public static Error CartNotFound =>
            Error.NotFound(code: "Cart.CartNotFound", description: "Cart is not found.");
        public static Error CartNotCreated =>
            Error.Failure(code: "Cart.CartNotCreated", description: "Failed to create Cart.");
        public static Error CartNotDeleted =>
            Error.Failure(code: "Cart.CartNotDeleted", description: "Failed to delete Cart.");
        public static Error CartPrductNotAdded =>
            Error.Failure(
                code: "Cart.CartPrductNotAdded",
                description: "Failed to add product to Cart."
            );
        public static Error CartPrductNotFound =>
            Error.NotFound(code: "User.CartImageNotFound", description: "Cart image is not found.");
        public static Error CartNotUpdated =>
            Error.Failure(code: "Cart.CartNotUpdated", description: "Failed to update Cart.");
        public static Error CartPrductNotRemoved =>
            Error.Failure(
                code: "Cart.CartPrductNotRemoved",
                description: "Failed to remove product from Cart."
            );
        public static Error ProductAlreadyExistInCart =>
            Error.Failure(
                code: "Cart.ProductAlreadyExistInCart",
                description: "Product is already exist in Cart."
            );
    }
}

