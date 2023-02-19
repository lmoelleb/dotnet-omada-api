namespace OmadaApi.Generator.Tests.DocumentationReader;

using System.IO;
using System.Linq;
using FluentAssertions;
using OmadaApi.Generator.ApiDocumentationReader;
using Xunit;

public class ApiDocumentationSectionTests
{
    private static readonly string TestFileContent = File.ReadAllText("Omada_SDN_Controller_V5.4.6 API Document.html");

    /* Not a true unit test as I parse the entire file - these test can break if classes
     * earlier in the pipeline fails.
    */

    [Fact]
    public void FirstAndLastEndPointAvailable()
    {
        ApiDocumentation apiDocumentation = new ApiDocumentation(TestFileContent);
        var adminsSection = apiDocumentation.Sections.First(s => s.Title == "Admins");
        adminsSection.Endpoints.Should().Contain(ep => ep.Title == "Get Information of Current Admin Account", because: "the first endpoint should be included");
        adminsSection.Endpoints.Should().Contain(ep => ep.Title == "Send User Reviews", because: "the last endpoint should be included");
    }
}
