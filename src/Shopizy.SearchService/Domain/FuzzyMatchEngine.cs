namespace Shopizy.SearchService.Domain;

public static class FuzzyMatchEngine
{
    public static int DamerauLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        source = source.ToLowerInvariant();
        target = target.ToLowerInvariant();

        int length1 = source.Length;
        int length2 = target.Length;
        var matrix = new int[length1 + 1, length2 + 1];

        for (int i = 0; i <= length1; i++) matrix[i, 0] = i;
        for (int j = 0; j <= length2; j++) matrix[0, j] = j;

        for (int i = 1; i <= length1; i++)
        {
            for (int j = 1; j <= length2; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);

                if (i > 1 && j > 1 && source[i - 1] == target[j - 2] && source[i - 2] == target[j - 1])
                {
                    matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
                }
            }
        }

        return matrix[length1, length2];
    }

    public static bool IsFuzzyMatch(string queryTerm, string candidateWord, int maxDistance = 2)
    {
        if (string.IsNullOrWhiteSpace(queryTerm) || string.IsNullOrWhiteSpace(candidateWord))
            return false;

        queryTerm = queryTerm.Trim().ToLowerInvariant();
        candidateWord = candidateWord.Trim().ToLowerInvariant();

        if (candidateWord.Contains(queryTerm))
            return true;

        if (queryTerm.Length <= 3)
            return queryTerm == candidateWord;

        int distance = DamerauLevenshteinDistance(queryTerm, candidateWord);
        return distance <= (queryTerm.Length > 5 ? maxDistance : 1);
    }
}
