namespace OmadaApi.Generator.ApiDocumentationReader;

public record ApiObjectPropertyDocumentation
{
    public ApiObjectPropertyDocumentation(
        string name,
        string type,
        bool isRequired,
        string @default,
        string description,
        string other,
        ApiObjectDocumentation? complexType)
    {
        this.Name = name;
        this.Type = type;
        this.IsRequired = isRequired;
        this.Default = @default;
        this.Description = description;
        this.Other = other;
        this.ComplexType = complexType;
    }

    public string Name { get; }

    public string Type { get; }

    public bool IsRequired { get; }

    public string Default { get; }

    public string Description { get; }

    public string Other { get; }

    public string BaseType => this.Type.TrimEnd('[', ']', ' ');

    public bool IsComplexType => this.ComplexType is not null;

    public bool IsArray => this.Type.EndsWith("]");

    // Could have subtyped.... might still :D
    public ApiObjectDocumentation? ComplexType { get; }
}
