namespace OmadaApi.Generator.ApiDefinition;
using System.Collections.Generic;

internal class ApiClassDefinition
{
    public ApiClassDefinition(string name)
    {
        this.Name = name;
    }

    public string Name { get; }

    public IList<ApiClassDefinition> ClassProperties { get; } = new List<ApiClassDefinition>();

    public IList<(ApiClassDefinition ClassDefinition, string ParameterName)> ClassMethods { get; } = new List<(ApiClassDefinition, string)>();
}
