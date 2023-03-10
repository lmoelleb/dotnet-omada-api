namespace OmadaApi.Generator.ApiDocumentationReader;

public record ApiRequestPathParameterDocumentation
{
    public ApiRequestPathParameterDocumentation(string name, string example, string description)
    {
        this.Name = name;
        this.Example = example;
        this.Description = description;
    }

    public string Name { get; }

    public string Example { get; }

    public string Description { get; }
}
