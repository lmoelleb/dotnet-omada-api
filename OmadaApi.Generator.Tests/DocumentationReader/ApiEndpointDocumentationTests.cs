namespace OmadaApi.Generator.Tests.DocumentationReader;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using OmadaApi.Generator.ApiDocumentationReader;
using Xunit;

// Consider writing more specific tests that send nodes into the builder,
// instead of loading the full document.
public class ApiEndpointDocumentationTests
{
    private static readonly string TestFileContent = File.ReadAllText("Omada_SDN_Controller_V5.4.6 API Document.html");
    private static readonly ApiDocumentation OmadaApiDocumentation = new ApiDocumentation(TestFileContent);

    private IEnumerable<ApiEndpointDocumentation> AllEndpoints =>
        from section in OmadaApiDocumentation.Sections
        from endpoint in section.Endpoints
        select endpoint;

    /* Not a true unit test as I parse the entire file - these test can break if classes
     * earlier in the pipeline fails.
    */

    [Fact]
    public void PathAndMethodAreSet()
    {
        var endpoint = this.GetEndpoint("Edit an Admin Account");
        endpoint.Path.Should().Be("/{omadacId}/api/v2/users/{userID}");
        endpoint.HttpMethod.ToString().Should().Be("PATCH");
    }

    [Fact]
    public void PermissionsAreSet()
    {
        var endpoint = this.GetEndpoint("Edit an Admin Account");
        endpoint.Permissions.Should().Be("Login is required.", because: "hte permissions should be set");
    }

    [Fact]
    public void RequestHeadersAreSet()
    {
        var endpoint = this.GetEndpoint("Edit an Admin Account");
        endpoint.RequestHeaders.Should().HaveCount(1);
        var header = endpoint.RequestHeaders.Single();
        header.Name.Should().Be("Content-Type");
        header.Value.Should().Be("application/json");
        header.IsRequired.Should().BeTrue();

        // In case there is an endpoint with a description and/or example it should be used instead.
        header.Example.Should().BeEmpty();
        header.Description.Should().BeEmpty();
    }

    [Fact]
    public void RequestPathParametersAreSet()
    {
        var endpoint = this.GetEndpoint("Edit an Admin Account");
        endpoint.RequestPathParameters.Should().HaveCount(2);
        var controllerIdParameter = endpoint.RequestPathParameters.Single(p => p.Name == "omadacId");
        controllerIdParameter.Should().NotBeNull();
        controllerIdParameter.Description.Should().Be("Key ID of omadac");
    }

    [Fact]
    public void RequestQueryParametersAreSet()
    {
        var endpoint = this.GetEndpoint("Send SMS Verification Code (/portal/sendSmsAuthCode)");
        endpoint.RequestQueryParameters.Should().HaveCount(1);
        var queryParameter = endpoint.RequestQueryParameters.Single();
        queryParameter.Name.Should().Be("key");
        queryParameter.IsRequired.Should().BeTrue();
        queryParameter.Description.Should().StartWith("AES key encrypted");
    }

    [Fact]
    public void RequestBodyIsCreated()
    {
        var endpoint = this.GetEndpoint("Add a New Admin Account");
        endpoint.RequestBody.Should().NotBeNull();
        var body = endpoint.RequestBody!;

        // Should have had different unit tests focussing on the complext
        // objects.... but for now this will have to do.

        // Check a top level simple property
        var email = body.Properties.Single(p => p.Name == "email");
        email.Should().NotBeNull();
        email.IsRequired.Should().BeFalse();
        email.BaseType.Should().Be("string");
        email.Type.Should().Be("string");
        email.IsArray.Should().BeFalse();
        email.IsComplexType.Should().BeFalse();
        email.Description.Should().Be("When the type is 0, the entry indicates the email address of the Local Admin user.");

        // Check a complex type in the middle
        var privilege = body.Properties.Single(p => p.Name == "privilege");
        privilege.Should().NotBeNull();
        privilege.ComplexType.Should().NotBeNull();
        var sites = privilege.ComplexType!.Properties.Single(p => p.Name == "sites");
        sites.Should().NotBeNull();
        sites.BaseType.Should().Be("object");
        sites.IsComplexType.Should().BeTrue();
        sites.IsArray.Should().BeTrue();
        sites.Other.Should().Be("item Type: object");
    }

    [Fact]
    public void ResponseBodyShouldBeSet()
    {
        var endpoint = this.GetEndpoint("Edit an Admin Account");
        var body = endpoint.ResponseBody;
        body.Should().NotBeNull();

        // Just a basic test. The code building the response body
        // is the same as for the request so tested elsewhere
        body!.Properties.Should().Contain(p => p.Name == "errorCode" && p.IsRequired);
    }

    private ApiEndpointDocumentation GetEndpoint(string title) =>
                this.AllEndpoints.SingleOrDefault(ep => ep.Title == title) ?? throw new ArgumentException($"Unable to find endpoint with title: {title}", nameof(title));
}
