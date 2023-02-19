namespace OmadaApi.Generator.Tests.DocumentationReader;

using System.IO;
using FluentAssertions;
using OmadaApi.Generator.ApiDocumentationReader;
using Xunit;

public class ApiDocumentationTests
{
    private static readonly string TestFile = File.ReadAllText("Omada_SDN_Controller_V5.4.6 API Document.html");

    [Fact]
    public void VersionIsExtracted()
    {
        var omadaApiDocumentation = new ApiDocumentation(TestFile);
        omadaApiDocumentation.Version.Should().Be("5.4.6", because: "the version should be extracted from the html/head/title element");
    }

    [Fact]
    public void FindsFirstAndLastSection()
    {
        var uot = new ApiDocumentation(TestFile);
        uot.Sections.Should().NotBeEmpty();
        uot.Sections.Should().Contain(s => s.Title == "Admins", because: "the first section should be found");
        uot.Sections.Should().Contain(s => s.Title == "Statistic", because: "the last section should be found");
    }
}