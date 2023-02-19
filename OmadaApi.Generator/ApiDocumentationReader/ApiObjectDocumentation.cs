namespace OmadaApi.Generator.ApiDocumentationReader
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public record ApiObjectDocumentation
    {
        public ApiObjectDocumentation(IReadOnlyCollection<ApiObjectPropertyDocumentation> properties)
        {
            this.Properties = properties;
        }

        public IReadOnlyCollection<ApiObjectPropertyDocumentation> Properties { get; }
    }
}
