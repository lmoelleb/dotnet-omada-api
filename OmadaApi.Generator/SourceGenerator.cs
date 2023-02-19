namespace Bevenel.OmadaApi.Generator;

using System;
using global::OmadaApi.Generator.ApiDocumentationReader;
using Microsoft.CodeAnalysis;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> apiDocuments = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".html"));

        IncrementalValuesProvider<string> apiDocumentations = apiDocuments.Select((text, cancellationToken) => text.GetText(cancellationToken)!.ToString());

        var generate = (Action<SourceProductionContext, string>)((spc, apiDocumentation) =>
        {
            ApiDocumentation apiDoc = new(apiDocumentation);

            string source = this.GenerateSource(apiDoc);
            spc.AddSource($"Bevenel.OmadaApi.V{apiDoc.Version}", source);
        });

        context.RegisterSourceOutput(apiDocumentations, generate);
    }

    private string GenerateSource(ApiDocumentation apiDocumentation)
    {
        return $"""
                namespace Bevenel.OmadaApi.V{apiDocumentation.Version};
                """;
    }
}
