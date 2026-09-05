using System.Net.Mail;
using System.Text.RegularExpressions;
using Shopizy.SharedKernel.Domain;
using Shopizy.SharedKernel.Results;

namespace Shopizy.IdentityService.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = MyRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(Error.Validation("Email.Empty", "Email address cannot be empty."));
        }

        var trimmed = email.Trim().ToLowerInvariant();

        if (trimmed.Length > 256)
        {
            return Result.Failure<Email>(Error.Validation("Email.TooLong", "Email address must not exceed 256 characters."));
        }

        if (!EmailRegex.IsMatch(trimmed))
        {
            return Result.Failure<Email>(Error.Validation("Email.InvalidFormat", "Email address has an invalid format."));
        }

        try
        {
            _ = new MailAddress(trimmed);
        }
        catch
        {
            return Result.Failure<Email>(Error.Validation("Email.InvalidFormat", "Email address has an invalid format."));
        }

        return Result.Success(new Email(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex MyRegex();
}
