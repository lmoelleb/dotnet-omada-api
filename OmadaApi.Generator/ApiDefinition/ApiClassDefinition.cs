namespace OmadaApi.Generator.ApiDefinition;
using System.Collections.Generic;

internal class ApiClassDefinition
{
    public ApiClassDefinition(string name)
    {
        this.Name = name;
    }

    public string Name { get; }

    public IList<ApiTransitionalClassProperty> ClassProperties { get; } = new List<ApiTransitionalClassProperty>();

    public IList<ApiTransitionalPathMethod> ClassMethods { get; } = new List<ApiTransitionalPathMethod>();
}
