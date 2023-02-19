namespace OmadaApi.Generator.ApiDocumentationReader;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

internal class ApiDocumentation
{
    private readonly Lazy<HtmlDocument> doc;
    private readonly Lazy<string> version;
    private readonly Lazy<IReadOnlyCollection<ApiDocumentationSection>> sections;

    public ApiDocumentation(string htmlDocumentation)
    {
        this.doc = new Lazy<HtmlDocument>(() =>
        {
            var d = new HtmlDocument();
            d.LoadHtml(htmlDocumentation);
            return d;
        });

        this.version = new Lazy<string>(this.ExtractVersion);
        this.sections = new Lazy<IReadOnlyCollection<ApiDocumentationSection>>(() => ApiDocumentationSection.Create(this.doc.Value));
    }

    public string Version => this.version.Value;

    public IReadOnlyCollection<ApiDocumentationSection> Sections => this.sections.Value;

    private string ExtractVersion()
    {
        // Not expected to run often, so keep variable instead of litering the code with fields.
        string versionGroupname = "version";
        Regex versionFromTitleRegex = new Regex($@"(\s|^|_)V(?'{versionGroupname}'\d+(\.\d+)+)(\s|_|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var titleNode = this.doc.Value.DocumentNode.SelectSingleNode("/html/head/title");
        if (titleNode == null)
        {
            throw new NotSupportedException($"Missing head/title element");
        }

        var match = versionFromTitleRegex.Match(titleNode.InnerText.Trim());
        if (!match.Success)
        {
            throw new NotSupportedException($"The head/title element does not contain a version on the form Vx.x.x");
        }

        return match.Groups[versionGroupname].Value;
    }
}