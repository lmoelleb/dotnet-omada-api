namespace OmadaApi.Generator.Definition
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public record ApiObject
    {
        public ApiObject(IReadOnlyCollection<ApiObjectProperty> properties)
        {
            this.Properties = properties;
        }

        public IReadOnlyCollection<ApiObjectProperty> Properties { get; }
    }
}
