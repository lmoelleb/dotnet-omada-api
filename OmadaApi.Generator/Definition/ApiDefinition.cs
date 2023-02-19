namespace OmadaApi.Generator.Definition;

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

internal class ApiDefinition
{
    private readonly Lazy<HtmlDocument> doc;
    private readonly Lazy<string> version;
    private readonly Lazy<IReadOnlyCollection<ApiSection>> sections;

    public ApiDefinition(string htmlDefinition)
    {
        this.doc = new Lazy<HtmlDocument>(() =>
        {
            var d = new HtmlDocument();
            d.LoadHtml(htmlDefinition);
            return d;
        });

        this.version = new Lazy<string>(this.ExtractVersion);
        this.sections = new Lazy<IReadOnlyCollection<ApiSection>>(() => ApiSection.Create(this.doc.Value));
    }

    public string Version => this.version.Value;

    public IReadOnlyCollection<ApiSection> Sections => this.sections.Value;

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