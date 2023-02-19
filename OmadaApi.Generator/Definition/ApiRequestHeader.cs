namespace OmadaApi.Generator.Definition;

public record ApiRequestHeader
{
    public ApiRequestHeader(string name, string value, bool isRequired, string example, string description)
    {
        this.Name = name;
        this.Value = value;
        this.IsRequired = isRequired;
        this.Example = example;
        this.Description = description;
    }

    public string Name { get; }

    public string Value { get; }

    public bool IsRequired { get; }

    public string Example { get; }

    public string Description { get; }
}
