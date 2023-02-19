namespace OmadaApi.Tests;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Codenizer.HttpClient.Testable;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OmadaApi.Implementation;
using Xunit;

public class ContextTests
{
    private const string DefaultControllerId = "controllerid42";
    private const string DefaultToken = "ChuckNorrisWantsAccess";

    [Fact]
    public void ExtraPartsOfBaseUriStrippedInUriBuilder()
    {
        var setup = Setup("https://some.place:42/PathWillBeIgnored?andSoWill=QueryParamerters");
        var uri = setup.Context.CreateUriBuilder("MyNewPath").Build();
        uri.ToString().Should().Be("https://some.place:42/MyNewPath");
    }

    [Fact]
    public void PathParametersAreReplacedInUriBuilder()
    {
        var setup = Setup("https://no.where");
        var uri = setup.Context.CreateUriBuilder("{root}/{middleish}withadded/constant/{end}")
            .WithPathParameter("root", "start")
            .WithPathParameter("middleish", "centerish")
            .WithPathParameter("end", "tail")
            .Build();

        uri.ToString().Should().Be("https://no.where/start/centerishwithadded/constant/tail");
    }

    [Fact]
    public void QueryParametersAreAddedInUriBuilder()
    {
        var setup = Setup("https://no.where");
        var uri = setup.Context.CreateUriBuilder("path")
            .WithQueryParameter("escapable", "&")
            .WithQueryParameter("duplicate", "willBeOverwritten")
            .WithQueryParameter("duplicate", "expectedValue")
            .Build();

        var queryParms = HttpUtility.ParseQueryString(uri.Query);
        queryParms.Count.Should().Be(2);
        queryParms["escapable"].Should().Be("&");
        queryParms["duplicate"].Should().Be("expectedValue");
    }

    [Fact]
    public void UriBuilderThrowsOnBuildIfPathParametersMissing()
    {
        var setup = Setup("https://no.where");

        var uriBuilder = setup.Context.CreateUriBuilder("path/{missing}/end");

        uriBuilder.Invoking(b => b.Build()).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task RequestRetrievesControllerIdIfRequireds()
    {
        string baseUrl = "https://no.where";
        string expected = "the result";
        var setup = Setup(baseUrl);
        var urlBuilder = setup.Context.CreateUriBuilder("{omadacId}/getstring");
        setup.MessageHander.RespondTo()
            .Get()
            .ForUrl($"{baseUrl}/{DefaultControllerId}/getstring")
            .With(HttpStatusCode.OK)
            .AndJsonContent(new Response<string> { ErrorCode = 0, Result = expected });

        var received = await setup.Context.GetAsync<string>(urlBuilder);

        received.Should().Be(expected);
    }

    [Fact]
    public async Task RequestLogsInIfRequired()
    {
        string baseUrl = "https://no.where";
        string expected = "the result";
        string username = "ChuckNorris";
        string password = "Don'tNeedNoStinkingPassword";
        var setup = Setup(baseUrl);
        var context = setup.Context.WithCredentials(username, password);

        // Leave controller ID out on purpose, it should figure out it needs to request it for the login
        var urlBuilder = context.CreateUriBuilder("getstring");
        setup.MessageHander.RespondTo()
            .Get()
            .ForUrl(urlBuilder.Build().ToString() + "?token=" + DefaultToken)
            .With(HttpStatusCode.OK)
            .AndJsonContent(new Response<string> { ErrorCode = 0, Result = expected });

        var received = await context.GetAsync<string>(urlBuilder);
        received.Should().Be(expected);
        var loginRequest = setup.MessageHander.Requests.FirstOrDefault(r => r.RequestUri?.PathAndQuery.Contains("login") ?? false);

        // Would have liked to test the content of the call to login, but not sure if I can
        // (stream already processed) so for now assume the call is OK
        loginRequest.Should().NotBeNull(because: "there should have been a call to login");

        var getStringRequest = setup.MessageHander.Requests.FirstOrDefault(r => r.RequestUri?.PathAndQuery.Contains("getstring") ?? false);
        getStringRequest.Should().NotBeNull();
        getStringRequest!.RequestUri.Should().NotBeNull();
        getStringRequest.RequestUri!.Query.Should().Contain($"token={DefaultToken}"); // not 100% check but will do
        getStringRequest.Headers.Accept.Should().HaveCount(1).And.AllSatisfy(a => a.MediaType.Should().Be("application/json"));
    }

    private static (Context Context, TestableMessageHandler MessageHander) Setup(string baseUri)
    {
        var messageHandler = new TestableMessageHandler();
        var clientFactory = Substitute.For<IHttpClientFactory>();

        Response<ControllerInfo> controllerInfoResponse = new()
        {
            ErrorCode = 0,
            Result = new ControllerInfo
            {
                ApiVer = "3",
                ControllerVer = "5.4.6",
                Type = 10, // No idea what this is
                OmadaCId = DefaultControllerId,
                Configured = true,
                SupportApp = true,
            },
        };

        messageHandler.RespondTo()
            .Get()
            .ForUrl(baseUri + "/api/info")
            .With(HttpStatusCode.OK)
            .AndJsonContent(controllerInfoResponse);

        Response<LoginResult> loginResponse = new()
        {
            ErrorCode = 0,
            Result = new LoginResult { Token = DefaultToken },
        };

        messageHandler.RespondTo()
            .Post()
            .ForUrl($"{baseUri}/{DefaultControllerId}/api/v2/login")
            .With(HttpStatusCode.OK)
            .AndJsonContent(loginResponse);

        // Keeps creating HttpClient which might not be ideal
        clientFactory.CreateClient("Bevenel.OmadaApi").Returns(new HttpClient(messageHandler));
        var loggerFactory = NullLoggerFactory.Instance;

        var context = new Context(new Uri(baseUri), clientFactory, loggerFactory);

        return (context, messageHandler);
    }
}