namespace OmadaApi.Generator.DocumentationToDefinition;
internal record PathToken
{
    public PathToken(string name, string? parameterName)
    {
        this.Name = name;
        this.ParameterName = parameterName?.Trim('{', '}');
    }

    public string Name { get; }

    public string? ParameterName { get; }

    public bool HasParameter => !string.IsNullOrEmpty(this.ParameterName);

    public override string ToString()
    {
        return this.Name + (this.HasParameter ? $"/{{{this.ParameterName}}}" : string.Empty);
    }
}
