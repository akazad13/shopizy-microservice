using Shopizy.SharedKernel.Results;

namespace Shopizy.IdentityService.Domain.Rules;

public static class PasswordPolicy
{
    public const int MinimumLength = 12;

    public static Result Validate(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return Result.Failure(Error.Validation("Password.Empty", "Password cannot be empty."));
        }

        if (password.Length < MinimumLength)
        {
            return Result.Failure(Error.Validation("Password.TooShort", $"Password must be at least {MinimumLength} characters long."));
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        if (!hasUpper)
        {
            return Result.Failure(Error.Validation("Password.MissingUppercase", "Password must contain at least one uppercase letter."));
        }

        if (!hasLower)
        {
            return Result.Failure(Error.Validation("Password.MissingLowercase", "Password must contain at least one lowercase letter."));
        }

        if (!hasDigit)
        {
            return Result.Failure(Error.Validation("Password.MissingDigit", "Password must contain at least one numeric digit."));
        }

        if (!hasSpecial)
        {
            return Result.Failure(Error.Validation("Password.MissingSpecial", "Password must contain at least one special character."));
        }

        return Result.Success();
    }
}
