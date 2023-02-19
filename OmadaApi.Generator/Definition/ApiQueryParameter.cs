namespace OmadaApi.Generator.Definition;

public record ApiQueryParameter
{
    public ApiQueryParameter(string name, bool isRequired, string example, string description)
    {
        this.Name = name;
        this.IsRequired = isRequired;
        this.Example = example;
        this.Description = description;
    }

    public string Name { get; }

    public bool IsRequired { get; }

    public string Example { get; }

    public string Description { get; }
}
