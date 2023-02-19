namespace OmadaApi.Generator.Definition;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using HtmlAgilityPack;

internal class ApiSection
{
    private ApiSection(string title, IReadOnlyCollection<ApiEndpoint> endpoints)
    {
        this.Title = title;
        this.Endpoints = endpoints;
    }

    public string Title { get; }

    public IReadOnlyCollection<ApiEndpoint> Endpoints { get; }

    public static IReadOnlyCollection<ApiSection> Create(HtmlDocument htmlDocument)
    {
        List<Builder> builders = new List<Builder>();

        // Assumption: an h1 without a class is a new section

        // Manually select the first node to get the right level to enumerate siblings on.
        var node = htmlDocument.DocumentNode.SelectSingleNode("//h1[not(@class)]");
        while (node != null)
        {
            if (node.Name == "h1" && string.IsNullOrEmpty(node.GetAttributeValue("class", string.Empty)))
            {
                builders.Add(new Builder(node.InnerText));
            }

            if (builders.Any())
            {
                builders.Last().ProcessHtmlNode(node);
            }

            node = node.NextSibling;
        }

        return builders.Select(b => b.Build()).ToImmutableArray();
    }

    internal class Builder
    {
        private readonly List<ApiEndpoint.Builder> endpoints = new();
        private string? endpointTitleCandidate;

        public Builder(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
            }

            this.Title = title;
        }

        public string Title { get; }

        public ApiSection Build() =>
            new ApiSection(
                this.Title,
                this.endpoints.Select(epb => epb.Build()).ToImmutableArray());

        public void ProcessHtmlNode(HtmlNode node)
        {
            // Not sure what happened to the language list in the docs :)
            if (node.Name == "h2")
            {
                this.endpointTitleCandidate = node.InnerText;
            }
            else if (node.Name == "h3" && node.InnerText == "Basic Information")
            {
                if (string.IsNullOrEmpty(this.endpointTitleCandidate))
                {
                    throw new InvalidOperationException("Unable to create new endpoint as the title has not been set");
                }

                this.endpoints.Add(new ApiEndpoint.Builder(this.endpointTitleCandidate!));
                this.endpointTitleCandidate = null;
            }

            this.endpoints.LastOrDefault()?.ProcessHtmlNode(node);
        }
    }
}
