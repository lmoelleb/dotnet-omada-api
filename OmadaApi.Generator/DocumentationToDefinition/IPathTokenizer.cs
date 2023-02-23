namespace OmadaApi.Generator.DocumentationToDefinition;

using System.Collections.Generic;

internal interface IPathTokenizer
{
    IReadOnlyList<PathToken> GetTokens(string path);
}