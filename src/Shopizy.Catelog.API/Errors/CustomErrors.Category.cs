using ErrorOr;

namespace Shopizy.Catelog.API.Errors;

public static partial class CustomErrors
{
    public static class Category
    {
        public static Error DuplicateName =>
            Error.Conflict(
                code: "Category.DuplicateName",
                description: "Category with the same name is already in use."
            );
        public static Error CategoryNotFound =>
            Error.NotFound(code: "User.CategoryNotFound", description: "Category is not found.");
        public static Error CategoryNotCreated =>
            Error.Failure(
                code: "Category.CategoryNotCreated",
                description: "Failed to create Category."
            );
        public static Error CategoryNotUpdated =>
            Error.Failure(
                code: "Category.CategoryNotUpdated",
                description: "Failed to update Category."
            );
        public static Error CategoryNotDeleted =>
            Error.Failure(
                code: "Category.CategoryNotDeleted",
                description: "Failed to delete Category."
            );
    }
}

