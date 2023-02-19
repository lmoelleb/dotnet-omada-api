namespace OmadaApi.Generator.ApiDocumentationReader
{
    using System.Collections.Generic;

    public record ApiObjectDocumentation
    {
        public ApiObjectDocumentation(IReadOnlyCollection<ApiObjectPropertyDocumentation> properties)
        {
            this.Properties = properties;
        }

        public IReadOnlyCollection<ApiObjectPropertyDocumentation> Properties { get; }
    }
}
