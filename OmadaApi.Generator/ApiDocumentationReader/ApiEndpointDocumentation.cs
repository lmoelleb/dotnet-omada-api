namespace OmadaApi.Generator.ApiDocumentationReader;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

public class ApiEndpointDocumentation
{
    private static readonly Regex IndentLevelRegex = new Regex(
        @"(^|;)\s*padding-left\s*:\s*(?'pixels'\d+)\s*px\s*(;|$)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private ApiEndpointDocumentation(
        string title,
        string path,
        HttpMethod httpMethod,
        string permissions,
        IReadOnlyCollection<ApiErrorMessageDocumentation> errorMessages,
        IReadOnlyCollection<ApiRequestHeaderDocumentation> requestHeaders,
        IReadOnlyCollection<ApiRequestPathParameterDocumentation> requestPathParameters,
        IReadOnlyCollection<ApiQueryParameterDocumentation> queryParameters,
        ApiObjectDocumentation? requestBody,
        ApiObjectDocumentation? responseBody)
    {
        this.Title = title;
        this.Path = path;
        this.HttpMethod = httpMethod;
        this.Permissions = permissions;
        this.ErrorMessages = errorMessages;
        this.RequestHeaders = requestHeaders;
        this.RequestPathParameters = requestPathParameters;
        this.RequestQueryParameters = queryParameters;
        this.RequestBody = requestBody;
        this.ResponseBody = responseBody;
    }

    public string Title { get; }

    public string Path { get; }

    public HttpMethod HttpMethod { get; }

    public string Permissions { get; }

    public IReadOnlyCollection<ApiRequestHeaderDocumentation> RequestHeaders { get; }

    public IReadOnlyCollection<ApiRequestPathParameterDocumentation> RequestPathParameters { get; }

    public IReadOnlyCollection<ApiQueryParameterDocumentation> RequestQueryParameters { get; }

    public ApiObjectDocumentation? RequestBody { get; }

    public ApiObjectDocumentation? ResponseBody { get; }

    public IReadOnlyCollection<ApiErrorMessageDocumentation> ErrorMessages { get; }

    public class Builder
    {
        private readonly string title;
        private string path = string.Empty;
        private string method = string.Empty;
        private string permissions = string.Empty; // For now asume single string
        private ImmutableArray<ApiErrorMessageDocumentation> errorMessages = ImmutableArray<ApiErrorMessageDocumentation>.Empty;
        private ImmutableArray<ApiRequestHeaderDocumentation> requestHeaders = ImmutableArray<ApiRequestHeaderDocumentation>.Empty;
        private ImmutableArray<ApiRequestPathParameterDocumentation> requestPathParameters = ImmutableArray<ApiRequestPathParameterDocumentation>.Empty;
        private ImmutableArray<ApiQueryParameterDocumentation> requestQueryParameters = ImmutableArray<ApiQueryParameterDocumentation>.Empty;
        private ApiObjectDocumentation? requestBody;
        private ApiObjectDocumentation? responseBody;
        private string currentSubSection = string.Empty;

        public Builder(string title)
        {
            this.title = title.Trim();
        }

        public ApiEndpointDocumentation Build()
        {
            if (string.IsNullOrEmpty(this.path))
            {
                throw new InvalidOperationException($"Unable to build endpoint {this.title} as the {nameof(this.path)} has not been set.");
            }

            if (string.IsNullOrEmpty(this.method))
            {
                throw new InvalidOperationException($"Unable to build endpoint {this.title} as the {nameof(this.method)} has not been set.");
            }

            return new ApiEndpointDocumentation(
                this.title,
                this.path,
                new HttpMethod(this.method),
                this.permissions,
                this.errorMessages,
                this.requestHeaders,
                this.requestPathParameters,
                this.requestQueryParameters,
                this.requestBody,
                this.responseBody);
        }

        public void ProcessHtmlNode(HtmlNode node)
        {
            if (node.Name == "h3")
            {
                this.currentSubSection = node.InnerText;
            }

            this.ProcessParagraphElement(node);
            this.ProcessTableElement(node);
        }

        private void ProcessTableElement(HtmlNode node)
        {
            if (!node.Name.Equals("table", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var tableHeaders = this.GetTableHeaders(node);

            // Should probably gave looked at the preceding P element in most
            // cases, but for now try to wing it from the table format.
            if (this.currentSubSection == "Basic Information")
            {
                if (this.AreHeadersEqual(tableHeaders, "Permission"))
                {
                    this.ProcessPermissionTable(node);
                }

                if (this.AreHeadersEqual(tableHeaders, "Error Code", "Error Message"))
                {
                    this.ProcessErrorMessageTable(node);
                }
            }

            if (this.currentSubSection == "Request Parameters")
            {
                if (this.AreHeadersEqual(tableHeaders, "Parameters", "Value", "Required", "Example", "Description"))
                {
                    this.ProcessRequestHeaders(node);
                }

                if (this.AreHeadersEqual(tableHeaders, "Parameters", "Example", "Description"))
                {
                    this.ProcessRequestPathParameters(node);
                }

                if (this.AreHeadersEqual(tableHeaders, "Parameters", "Required", "Example", "Description"))
                {
                    this.ProcessQueryPathParameters(node);
                }

                if (this.AreHeadersEqual(tableHeaders, "Parameters", "Type", "Required", "Default", "Description", "Others"))
                {
                    this.requestBody = this.CreateApiObject(node);
                }
            }

            if (this.currentSubSection == "Response Parameters" &&
                this.AreHeadersEqual(tableHeaders, "Parameters", "Type", "Required", "Default", "Description", "Others"))
            {
                this.responseBody = this.CreateApiObject(node);
            }
        }

        private ApiObjectDocumentation? CreateApiObject(HtmlNode node)
        {
            Stack<List<ApiObjectPropertyDocumentation>> workStack = new();

            var rows = node.SelectNodes("tbody/tr");
            if (!(rows?.Any() ?? false))
            {
                // Could be an error in the API docs, but for now just accept there is no return body
                return null;
            }

            // Run the nodes in reverse, then all properties of an object are known when the object needs to be created
            foreach (var row in rows.Reverse())
            {
                string name = row.SelectSingleNode("td[1]").InnerText;
                string type = row.SelectSingleNode("td[2]").InnerText;
                bool isRequired = "yes".Equals(row.SelectSingleNode("td[3]").InnerText, StringComparison.OrdinalIgnoreCase);
                string @default = row.SelectSingleNode("td[4]").InnerText;
                string description = row.SelectSingleNode("td[5]").InnerText;
                string other = row.SelectSingleNode("td[6]").InnerText;
                int indentLevel = this.GetIndentLevel(row.SelectSingleNode("td[1]"));
                ApiObjectDocumentation? childObject = null;

                name = name.TrimStart(' ', '├', '─');

                // Push an extra level as we need one for indent = 0
                while (indentLevel >= workStack.Count)
                {
                    workStack.Push(new());
                }

                if (indentLevel < workStack.Count - 1)
                {
                    childObject = Pop();
                }

                workStack.Peek().Add(new ApiObjectPropertyDocumentation(
                    name,
                    type,
                    isRequired,
                    @default,
                    description,
                    other,
                    childObject));
            }

            var result = Pop();
            if (workStack.Any())
            {
                throw new NotSupportedException("Object hierarchy read error");
            }

            return result;

            ApiObjectDocumentation Pop()
            {
                var childProperties = workStack.Pop();
                childProperties.Reverse(); // As it is a list, the items are reversed in the list instance (unlike Linq Reverse)
                return new ApiObjectDocumentation(childProperties.ToImmutableArray());
            }
        }

        private int GetIndentLevel(HtmlNode parameterHtmlNode)
        {
            const int pixelsPerIndentLevel = 20;
            var spanStyle = parameterHtmlNode.SelectSingleNode("span")?.GetAttributeValue("style", string.Empty) ?? string.Empty;
            var match = IndentLevelRegex.Match(spanStyle);
            if (!match.Success)
            {
                return 0;
            }

            string pixelsText = match.Groups["pixels"].Value;

            return int.Parse(pixelsText, CultureInfo.InvariantCulture) / pixelsPerIndentLevel;
        }

        private void ProcessQueryPathParameters(HtmlNode node)
        {
            if (this.requestQueryParameters.Any())
            {
                throw new NotSupportedException("Request path parameters set twice");
            }

            this.requestQueryParameters = node.SelectNodes("tbody/tr").Select(row =>
                new ApiQueryParameterDocumentation(
                        row.SelectSingleNode("td[1]").InnerText,
                        "yes".Equals(row.SelectSingleNode("td[2]").InnerText.Trim(), StringComparison.OrdinalIgnoreCase),
                        row.SelectSingleNode("td[3]").InnerText,
                        row.SelectSingleNode("td[4]").InnerText)).ToImmutableArray();
        }

        private void ProcessRequestPathParameters(HtmlNode node)
        {
            if (this.requestPathParameters.Any())
            {
                throw new NotSupportedException("Request path parameters set twice");
            }

            this.requestPathParameters = node.SelectNodes("tbody/tr").Select(row =>
                new ApiRequestPathParameterDocumentation(
                        row.SelectSingleNode("td[1]").InnerText,
                        row.SelectSingleNode("td[2]").InnerText,
                        row.SelectSingleNode("td[3]").InnerText)).ToImmutableArray();
        }

        private void ProcessRequestHeaders(HtmlNode node)
        {
            if (this.requestHeaders.Any())
            {
                throw new NotSupportedException("Request headers set twice");
            }

            this.requestHeaders = node.SelectNodes("tbody/tr").Select(row =>
                new ApiRequestHeaderDocumentation(
                        row.SelectSingleNode("td[1]").InnerText,
                        row.SelectSingleNode("td[2]").InnerText,
                        "yes".Equals(row.SelectSingleNode("td[3]").InnerText, StringComparison.OrdinalIgnoreCase),
                        row.SelectSingleNode("td[4]").InnerText,
                        row.SelectSingleNode("td[5]").InnerText)).ToImmutableArray();
        }

        private void ProcessErrorMessageTable(HtmlNode node)
        {
            if (this.errorMessages.Any())
            {
                throw new NotSupportedException("Error messages set twice");
            }

            // let it throw on invalid int, will deal with it if needed
            this.errorMessages = (from row in node.SelectNodes("tbody/tr")
                                  let codeText = row.SelectSingleNode("td[1]").InnerText
                                  where !string.IsNullOrWhiteSpace(codeText)
                                  let code = int.Parse(codeText.Trim())
                                  let message = row.SelectSingleNode("td[2]").InnerText
                                  select new ApiErrorMessageDocumentation(code, message)).ToImmutableArray();
        }

        private void ProcessPermissionTable(HtmlNode node)
        {
            if (!string.IsNullOrEmpty(this.permissions))
            {
                throw new NotSupportedException("Permissions getting set twice");
            }

            var permissionNodes = node.SelectNodes("tbody/tr/td");
            if (permissionNodes.Count != 1)
            {
                throw new NotSupportedException("Expected a single permission text");
            }

            this.permissions = permissionNodes.Single().InnerText;
        }

        private bool AreHeadersEqual(IEnumerable<string> headers, params string[] expected)
        {
            // Seems C# 11 list patterns do not work in .NET Standard :(

            // Naive approach, speed is not important
            if (headers.Count() != expected.Length)
            {
                return false;
            }

            int index = 0;
            foreach (var header in headers)
            {
                if (!expected[index++].Equals(header, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private IReadOnlyList<string> GetTableHeaders(HtmlNode tableNode) =>
            (from tableHeaderCell in tableNode.SelectNodes("thead/tr/th")
             let text = tableHeaderCell.InnerText
             where !string.IsNullOrWhiteSpace(text)
             select text.Trim()).ToImmutableArray();

        private void ProcessParagraphElement(HtmlNode node)
        {
            if (!node.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Not checking very accurately, we will see if it is enough
            var headerNode = node.SelectSingleNode("strong");
            if (headerNode != null)
            {
                string headerText = headerNode.InnerText.Trim().Replace('：', ':');
                string value = headerNode.NextSibling?.InnerText.Trim() ?? string.Empty;
                switch (headerText)
                {
                    case "Path:":
                        this.path = value;
                        break;
                    case "Method:":
                        this.method = value;
                        break;
                }
            }
        }
    }
}
