namespace OmadaApi.Generator.ApiDefinition;

internal class ApiTransitionalClassProperty
{
    public ApiTransitionalClassProperty(string name, ApiClassDefinition classDefinition)
    {
        this.Name = name;
        this.ClassDefinition = classDefinition;
    }

    public string Name { get; set; }

    public ApiClassDefinition ClassDefinition { get; set; }

    public override string ToString() => $"{this.ClassDefinition.Name} {this.Name}";
}
