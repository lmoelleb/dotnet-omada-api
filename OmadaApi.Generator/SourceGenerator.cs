namespace Bevenel.OmadaApi.Generator;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using global::OmadaApi.Generator.Definition;
using Microsoft.CodeAnalysis;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> apiDefinitions = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".html"));

        IncrementalValuesProvider<string> apiDocumentations = apiDefinitions.Select((text, cancellationToken) => text.GetText(cancellationToken)!.ToString());

        context.RegisterSourceOutput(apiDocumentations, (spc, apiDocumentation) =>
        {
            ApiDefinition apiDefinition = new ApiDefinition(apiDocumentation);

            string source = this.GenerateSource(apiDefinition);
            spc.AddSource($"Bevenel.OmadaApi.V{apiDefinition.Version}", source);
        });
    }

    private string GenerateSource(ApiDefinition apiDefinition)
    {
        return $"""
                namespace Bevenel.OmadaApi.V{apiDefinition.Version};
                """;
    }
}
