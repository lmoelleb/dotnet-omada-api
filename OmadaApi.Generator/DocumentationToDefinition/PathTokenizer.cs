namespace OmadaApi.Generator.DocumentationToDefinition;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

internal class PathTokenizer
{
    private static readonly Regex SkipApiVersionRegex = new Regex(@"^/\{[^\}]+\}/api/v\d+/(?'rest'.+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IReadOnlyList<PathToken> GetTokens(string path)
    {
        var m = SkipApiVersionRegex.Match(path);
        if (!m.Success)
        {
            throw new ArgumentException("Expected a path on the form '{ctrlId}/api/vX/...' where X is an integer, got: " + path, nameof(path));
        }

        List<PathToken> pathTokens = new List<PathToken>();

        var relevantPartOfPath = m.Groups["rest"].Value;

        // The goal is to detect any place where a path parimeter selects a specific entity.
        // For example: /something/sites/{whatever}/ have sites/{whatever} mapped to a single token
        // We misued the fact that we know the path is ASCII and replace /{ with a placeholder
        // Could be done with regex or loops, but this is probably simpler
        var parts = relevantPartOfPath.Replace("/{", "•{").Split('/');
        foreach (var part in parts)
        {
            var split = part.Split('•');
            pathTokens.Add(new PathToken(split.First(), split.Skip(1).FirstOrDefault()));
        }

        return pathTokens.ToImmutableArray();
    }
}
