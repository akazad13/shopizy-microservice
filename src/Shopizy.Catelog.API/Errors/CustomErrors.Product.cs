using ErrorOr;

namespace Shopizy.Catelog.API.Errors;

public static partial class CustomErrors
{
    public static class Product
    {
        public static Error DuplicateName =>
            Error.Conflict(
                code: "Product.DuplicateName",
                description: "Product with the same name is already in use."
            );
        public static Error ProductNotFound =>
            Error.NotFound(code: "User.ProductNotFound", description: "Product is not found.");
        public static Error ProductNotCreated =>
            Error.Failure(
                code: "Product.ProductNotCreated",
                description: "Failed to create Product."
            );
        public static Error ProductNotUpdated =>
            Error.Failure(
                code: "Product.ProductNotUpdated",
                description: "Failed to update Product."
            );
        public static Error ProductNotDeleted =>
            Error.Failure(
                code: "Product.ProductNotDeleted",
                description: "Failed to delete Product."
            );
        public static Error ProductImageNotAdded =>
            Error.Failure(
                code: "Product.ProductImageNotAdded",
                description: "Failed to add Product image."
            );
        public static Error ProductImageNotUploaded =>
            Error.Failure(
                code: "Product.ProductImageNotUploaded",
                description: "Please upload a valid Product image."
            );
        public static Error ProductImageNotFound =>
            Error.NotFound(
                code: "User.ProductImageNotFound",
                description: "Product image is not found."
            );
    }
}
