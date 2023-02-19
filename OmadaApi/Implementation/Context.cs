namespace OmadaApi.Implementation;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmadaApi;

/// <summary>
/// Internal class reponsible for authentication as well as performing actual API calls.
/// </summary>
internal partial class Context
{
    private const string ControllerIdPathTokenName = "omadacId";
    private const string ControllerIdPathToken = "{" + ControllerIdPathTokenName + "}";

    private readonly string url;
    private readonly HttpClient httpClient;
    private readonly ILoggerFactory loggerFactory;

    private string? username;
    private string? password;
    private string? token;

    private Task<ControllerInfo>? controllerInfoTask;
    private Task<string?>? loginTask;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="url">
    ///     The base URL of the Omada Controller.
    ///     Any supplied path or query parameters will be discarded.</param>
    /// <param name="httpClientFactory">
    ///     The <see cref="IHttpClientFactory"/> that can be used to get the <see cref="HttpClient"/>
    ///     that will be used to access the Omada API.
    /// </param>
    /// <param name="loggerFactory">
    ///     An <see cref="ILoggerFactory"/> that can be used to create one or more <see cref="ILogger"/>s.
    /// </param>
    public Context(Uri url, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        : this(url.GetLeftPart(UriPartial.Authority), httpClientFactory.CreateClient("Bevenel.OmadaApi"), loggerFactory)
    {
    }

    private Context(string url, HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        this.url = url;
        this.loggerFactory = loggerFactory;
        this.httpClient = httpClient;
    }

    private enum AttemptLogin
    {
        Auto,
        Never,
    }

    /// <summary>
    /// Returns a new <see cref="Context"/> with the username/password set.
    /// </summary>
    /// <param name="username">The username to use when authenticating with the Omada API.</param>
    /// <param name="password">The password to use when authenticating with the Omada API.</param>
    /// <returns>
    ///     A new <see cref="Context"/> that can be used to make authenticated calls
    ///     to the Omada API using the supplied username/password.
    /// </returns>
    public Context WithCredentials(string username, string password)
    {
        var result = this.Clone();
        result.username = username;
        result.password = password;
        result.token = null;
        this.loginTask = null;
        return result;
    }

    /// <summary>
    /// Returns a new <see cref="Context"/> with the token set.
    /// Can be used as an alternative to <see cref="WithCredentials(string, string)"/>
    /// if the token has already been retrieved by other means.
    /// </summary>
    /// <param name="token">The token to use with the Omada API.</param>
    /// <returns>
    ///     A new <see cref="Context"/> that can be used to make authenticated calls
    ///     to the Omada API using the supplied token.
    /// </returns>
    public Context WithToken(string token)
    {
        var result = this.Clone();
        result.username = null;
        result.password = null;
        result.token = token;

        // Possible race condition if login is in progress - not expected so deal with it later.
        this.loginTask = null;
        return result;
    }

    /// <summary>
    /// Creates an <see cref="IOmadaUrlBuilder"/> that can be used to construct URLs for the Omada API.
    /// </summary>
    /// <param name="path">The path (so no protocol, domain name, port or query parameters).</param>
    /// <returns>A <see cref="IOmadaUrlBuilder"/> for the specified path.</returns>
    public IOmadaUrlBuilder CreateUriBuilder(string path) => new OmadaUrlBuilder(this.url, path);

    /// <summary>
    ///     Gets the result of calling /api/info on the controller. The result will be cached.
    /// </summary>
    /// <returns>
    ///     The <see cref="ControllerInfo"/> containing details about the Omada Controller.
    /// </returns>
    public Task<ControllerInfo> GetControllerInfoAsync()
    {
        if (this.controllerInfoTask?.IsFaulted ?? true)
        {
            // Do not support cancellation as we reuse any running task.
            this.controllerInfoTask = this.RequestAsync<ControllerInfo>(
                HttpMethod.Get,
                this.CreateUriBuilder("/api/info"),
                null,
                AttemptLogin.Never,
                CancellationToken.None);
        }

        return this.controllerInfoTask;
    }

    /// <summary>
    ///     Performs a <see cref="HttpMethod.Get"/> operation on the <see cref="OmadaApi"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the expected JSON response.
    ///     </typeparam>
    /// <param name="urlBuilder">
    ///     The <see cref="IOmadaUrlBuilder"/> defining the endpoint to call.
    /// </param>
    /// <param name="requestBody">
    ///     The request body (if any).
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> that can be used to request
    ///     cancelation of the request before it completes.
    /// </param>
    /// <returns>The response from the Omada API.</returns>
    public Task<T> GetAsync<T>(IOmadaUrlBuilder urlBuilder, object? requestBody = null, CancellationToken cancellationToken = default)
    {
        return this.RequestAsync<T>(HttpMethod.Get, urlBuilder, requestBody, AttemptLogin.Auto, cancellationToken);
    }

    private async Task<T> RequestAsync<T>(HttpMethod httpMethod, IOmadaUrlBuilder urlBuilder, object? requestBody, AttemptLogin attemptLogin, CancellationToken cancellationToken)
    {
        if (urlBuilder.RequiresControllerId)
        {
            var controllerInfo = await this.GetControllerInfoAsync();
            urlBuilder = urlBuilder.WithPathParameter(ControllerIdPathTokenName, controllerInfo.OmadaCId);
        }

        var requestToken = attemptLogin == AttemptLogin.Never ?
            null :
            await this.TryGetTokenAsync().ConfigureAwait(false);

        if (!string.IsNullOrEmpty(requestToken))
        {
            urlBuilder = urlBuilder.WithQueryParameter("token", requestToken);
        }

        Uri requestUrl;
        try
        {
            requestUrl = urlBuilder.Build();
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentException("The UrlBuilder is not fully initialized: " + ex.Message, nameof(urlBuilder), ex);
        }

        using HttpRequestMessage requestMessage = new(httpMethod, requestUrl);
        if (!string.IsNullOrEmpty(requestToken))
        {
            requestMessage.Headers.Add("Csrf-Token", requestToken);
        }

        requestMessage.Headers.Accept.Clear();

        // Could prolly move to default headers?
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (requestBody is not null)
        {
            requestMessage.Content = JsonContent.Create(requestBody, new MediaTypeHeaderValue("application/json"));
        }

        using var responseMessage = await this.httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

        Response<T>? responseObject;
        try
        {
            responseObject = await responseMessage.Content.ReadFromJsonAsync<Response<T>>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Unable to deserialize result from calling '{urlBuilder}': {ex.Message}", ex, responseMessage.StatusCode);
        }

        responseMessage.EnsureSuccessStatusCode();

        if (responseObject is null)
        {
            throw new HttpRequestException($"No response received when calling'{urlBuilder}'.", null, responseMessage.StatusCode);
        }

        if (responseObject.ErrorCode != 0)
        {
            throw new OmadaApiRequestException(
                httpMethod,
                responseMessage.StatusCode,
                responseObject.ErrorCode,
                responseObject.Msg,
                requestUrl.ToString());
        }

        if (responseObject.Result is null)
        {
            throw new HttpRequestException($"No response result received when calling'{urlBuilder}'.", null, responseMessage.StatusCode);
        }

        return responseObject.Result;
    }

    private Task<string?> TryGetTokenAsync()
    {
        // Should use Lazy to avoid race conditions, but they won't break anything, so let's
        // get this working first.
        if (this.loginTask?.IsFaulted ?? true)
        {
            this.loginTask = LoginAsync();
        }

        return this.loginTask;

        async Task<string?> LoginAsync()
        {
            if (!string.IsNullOrEmpty(this.token))
            {
                return this.token;
            }

            if (string.IsNullOrEmpty(this.username) || string.IsNullOrEmpty(this.password))
            {
                return null;
            }

            var loginUrl = this.CreateUriBuilder("{omadacId}/api/v2/login");
            LoginRequest request = new() { Password = this.password, Username = this.username };

            // The login task can be shared, so ignore cancellation token
            var loginResult = await this.RequestAsync<LoginResult>(HttpMethod.Post, loginUrl, request, AttemptLogin.Never, CancellationToken.None);

            return loginResult.Token;
        }
    }

    private Context Clone()
    {
        var result = new Context(this.url, this.httpClient, this.loggerFactory);
        result.username = this.username;
        result.password = this.password;
        result.token = this.token;

        // ControllerInfo and login tasks should be shared between contexts I think.
        // Requires more design and potential refactor, so ignore for now.
        return result;
    }

    private sealed partial class OmadaUrlBuilder : IOmadaUrlBuilder
    {
        private readonly string path;
        private readonly string baseUri;
        private readonly ImmutableDictionary<string, string> pathParameters;
        private readonly ImmutableDictionary<string, string> queryParameters;
        private readonly bool requiresControllerId;

        private Uri? cachedResult;

        public OmadaUrlBuilder(string baseUri, string path)
            : this(baseUri, path, ImmutableDictionary<string, string>.Empty, ImmutableDictionary<string, string>.Empty, path.Contains(ControllerIdPathToken))
        {
        }

        private OmadaUrlBuilder(string baseUri, string path, ImmutableDictionary<string, string> pathParameters, ImmutableDictionary<string, string> queryParameters, bool requiresControllerId)
        {
            this.baseUri = baseUri;
            this.path = path;
            this.pathParameters = pathParameters;
            this.queryParameters = queryParameters;
            this.requiresControllerId = requiresControllerId;
        }

        public bool RequiresControllerId => this.requiresControllerId;

        public Uri Build()
        {
            if (this.cachedResult is not null)
            {
                return this.cachedResult;
            }

            UriBuilder result = new UriBuilder(this.baseUri);

            result.Path = PathParameterReplacementRegex().Replace(this.path, m =>
            {
                string name = m.Groups["name"].Value;
                if (this.pathParameters.TryGetValue(name, out var value))
                {
                    return value;
                }

                if (name == ControllerIdPathToken)
                {
                    // This variable can be resolved automatically, so keep it if not set
                    return m.Value;
                }

                throw new InvalidOperationException($"Unable to build the URL for path '{this.path}' as the path parameter '{name}' has not been provided by calling {nameof(this.WithPathParameter)}.");
            });

            if (this.queryParameters.Any())
            {
                result.Query = "?" + string.Join("&", this.queryParameters.Select(qp => $"{qp.Key}={Uri.EscapeDataString(qp.Value)}"));
            }

            this.cachedResult = result.Uri;

            return this.cachedResult;
        }

        public IOmadaUrlBuilder WithPathParameter(string parameterName, string value)
        {
            if (!this.path.Contains($"{{{parameterName}}}"))
            {
                throw new ArgumentException($"The path '{this.path}' does not contain a path variable {{{parameterName}}}", nameof(parameterName));
            }

            return new OmadaUrlBuilder(
                this.baseUri,
                this.path,
                this.pathParameters.SetItem(parameterName, value),
                this.queryParameters,
                parameterName != ControllerIdPathTokenName && this.requiresControllerId);
        }

        public IOmadaUrlBuilder WithQueryParameter(string parameterName, string value)
        {
            return new OmadaUrlBuilder(
                this.baseUri,
                this.path,
                this.pathParameters,
                this.queryParameters.SetItem(parameterName, value),
                this.requiresControllerId);
        }

        [return: NotNull]
        public override string ToString() => this.Build().ToString();

        [GeneratedRegex(@"\{(?'name'[a-zA-Z0-9_-]+)\}", RegexOptions.None, "en-US")]
        private static partial Regex PathParameterReplacementRegex();
    }
}
