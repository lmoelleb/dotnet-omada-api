namespace OmadaApi.Generator.Tests;

using System.IO;
using System.Linq;
using FluentAssertions;
using OmadaApi.Generator.Definition;
using Xunit;

public class ApiSectionTests
{
    private static readonly string TestFileContent = File.ReadAllText("Omada_SDN_Controller_V5.4.6 API Document.html");

    /* Not a true unit test as I parse the entire file - these test can break if classes
     * earlier in the pipeline fails.
    */

    [Fact]
    public void FirstAndLastEndPointAvailable()
    {
        ApiDefinition omadaApiDefinition = new ApiDefinition(TestFileContent);
        var adminsSection = omadaApiDefinition.Sections.First(s => s.Title == "Admins");
        adminsSection.Endpoints.Should().Contain(ep => ep.Title == "Get Information of Current Admin Account", because: "the first endpoint should be included");
        adminsSection.Endpoints.Should().Contain(ep => ep.Title == "Send User Reviews", because: "the last endpoint should be included");
    }
}
