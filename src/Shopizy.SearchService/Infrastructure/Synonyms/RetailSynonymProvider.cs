using Shopizy.SearchService.Application.Interfaces;

namespace Shopizy.SearchService.Infrastructure.Synonyms;

public sealed class RetailSynonymProvider : ISynonymProvider
{
    private readonly Dictionary<string, List<string>> _synonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        { "sneakers", new() { "athletic shoes", "trainers", "sneakers", "running shoes" } },
        { "athletic shoes", new() { "sneakers", "trainers", "running shoes" } },
        { "trainers", new() { "sneakers", "athletic shoes" } },
        { "shades", new() { "sunglasses", "eyewear", "shades" } },
        { "sunglasses", new() { "shades", "eyewear" } },
        { "laptop", new() { "notebook", "computer", "macbook", "pc" } },
        { "notebook", new() { "laptop", "computer" } },
        { "phone", new() { "smartphone", "mobile", "iphone", "android" } },
        { "mobile", new() { "phone", "smartphone" } }
    };

    public IReadOnlyList<string> ExpandSynonyms(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<string>();

        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var expanded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in tokens)
        {
            expanded.Add(token);
            if (_synonyms.TryGetValue(token, out var matches))
            {
                foreach (var match in matches)
                {
                    expanded.Add(match);
                }
            }
        }

        return expanded.ToList();
    }
}
