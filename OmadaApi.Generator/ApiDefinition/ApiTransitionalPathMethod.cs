namespace OmadaApi.Generator.ApiDefinition;

internal class ApiTransitionalPathMethod
{
    public ApiTransitionalPathMethod(string name, string parameterName, ApiClassDefinition classDefinition)
    {
        this.Name = name;
        this.ParameterName = parameterName;
        this.ClassDefinition = classDefinition;
    }

    public string Name { get; set; }

    public string ParameterName { get; set; }

    public ApiClassDefinition ClassDefinition { get; set; }

    public override string ToString() => $"{this.ClassDefinition.Name} {this.Name}({this.ParameterName})";
}
