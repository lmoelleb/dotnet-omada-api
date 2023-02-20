namespace OmadaApi.Generator.Tests.DocumentationToDefinition;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using OmadaApi.Generator.DocumentationToDefinition;
using Xunit;

public class PathTokenizerTests
{
    private const string ValidPrefix = "/{omadacId}/api/v2";

    [Theory]
    [InlineData("/api/v2/something", "Must include controller ID path parameter.")]
    [InlineData("/{omadacId}/v2/something", "Must include /api/.")]
    [InlineData("/{omadacId}/api/something", "Must include API version")]
    public void ThrowsIfInvalidPrefix(string prefix, string because)
    {
        var tokenizer = new PathTokenizer();
        tokenizer.Invoking(t => t.GetTokens(prefix)).Should().Throw<ArgumentException>(because);
    }

    [Theory]
    [InlineData("/some/path/with/no/parameters", "some→path→with→no→parameters", "path with no parameters should tokenize correctly")]
    [InlineData("/short", "short", "path single part should tokenize correctly")]
    [InlineData("/sites/{sideId}/something", "sites•sideId→something", "Parameter at start should tokenize correcly")]
    [InlineData("/something/sites/{sideId}/something", "something→sites•sideId→something", "Parameter in middle")]
    [InlineData("/something/sites/{sideId}", "something→sites•sideId", "Parameter at end should tokenize correcly")]
    [InlineData("/sites/{sideId}/eap/{eapid}", "sites•sideId→eap•eapid", "Trailing parameters should tokenize correcly")]
    public void DecodesCorrectly(string pathWithoutPrefix, string expected, string because)
    {
        string path = ValidPrefix + pathWithoutPrefix;
        var tokenizer = new PathTokenizer();
        var tokens = tokenizer.GetTokens(path);

        // Avoid using /, {, or other things that might be in the original string to ensure it was tokenized
        var verifiableString = string.Join("→", tokens.Select(t => t.Name + (t.HasParameter ? $"•{t.ParameterName}" : string.Empty)));
        verifiableString.Should().Be(expected, because);
    }
}
