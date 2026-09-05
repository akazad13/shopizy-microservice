using FluentAssertions;
using Shopizy.SearchService.Infrastructure.Synonyms;
using Xunit;

namespace Shopizy.SearchService.UnitTests;

public class SearchEngineUnitTests
{
    [Theory]
    [InlineData("iphne", "iphone", 1)]
    [InlineData("samsung", "sumsung", 1)]
    [InlineData("sneakers", "snkeaers", 2)]
    public void DamerauLevenshteinDistance_CalculatesAccurateDistance(string s1, string s2, int expectedMaxDist)
    {
        int dist = FuzzyMatchEngine.DamerauLevenshteinDistance(s1, s2);
        dist.Should().BeLessThanOrEqualTo(expectedMaxDist);
    }

    [Theory]
    [InlineData("iphne", "iPhone 15 Pro", true)]
    [InlineData("runing", "Running Shoes", true)]
    [InlineData("keyboard", "Apple MacBook", false)]
    public void IsFuzzyMatch_DetectsIntendedWords(string query, string candidate, bool expected)
    {
        bool matches = false;
        foreach (var word in candidate.Split(' '))
        {
            if (FuzzyMatchEngine.IsFuzzyMatch(query, word))
            {
                matches = true;
                break;
            }
        }
        matches.Should().Be(expected);
    }

    [Fact]
    public void RetailSynonymProvider_ExpandsSynonymsCorrectly()
    {
        var provider = new RetailSynonymProvider();
        var synonyms = provider.ExpandSynonyms("sneakers");

        synonyms.Should().Contain("sneakers");
        synonyms.Should().Contain("athletic shoes");
        synonyms.Should().Contain("trainers");
    }

    [Fact]
    public void RetailSynonymProvider_HandlesEmptyQueryGracefully()
    {
        var provider = new RetailSynonymProvider();
        var synonyms = provider.ExpandSynonyms("");
        synonyms.Should().BeEmpty();
    }
}
