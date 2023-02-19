namespace OmadaApi;

using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OmadaApi.Implementation;

/// <summary>
///     A factory used for creating API connections to
///     one or more TP-Link Omada controllers.
/// </summary>
public class OmadaControllerFactory
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILoggerFactory loggerFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OmadaControllerFactory"/> class
    ///     providing API access to a TP-Link Omada controller.
    /// </summary>
    /// <param name="httpClientFactory">
    ///     The <see cref="IHttpClientFactory"/> the API can use to create
    ///     a <see cref="HttpClient"/> used when calling the Omada API.
    /// </param>
    /// <param name="loggerFactory">
    ///     The <see cref="ILoggerFactory"/> used to log.
    /// </param>
    public OmadaControllerFactory(IHttpClientFactory httpClientFactory, ILoggerFactory? loggerFactory = null)
    {
        this.httpClientFactory = httpClientFactory;
        this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    /// <summary>
    ///     Creates a new API instance allowing communication with a TP-Link Omada controller.
    /// </summary>
    /// <param name="omadaUri">
    ///     The URI (hostname) of the TP-Link Omada controller.
    /// </param>
    /// <returns>
    ///     The <see cref="IOmadaController"/> that can be used to communicate with a single TP-Link Omada controller.
    /// </returns>
    public IUnauthenticatedOmadaController Create(Uri omadaUri) =>
        new Implementation.UnauthenticatedOmadaController(new Context(omadaUri, this.httpClientFactory, this.loggerFactory));

    /// <summary>
    ///     Creates a new API instance allowing communication with a TP-Link Omada controller.
    /// </summary>
    /// <param name="omadaUri">
    ///     The URI (hostname) of the TP-Link Omada controller.
    /// </param>
    /// <returns>
    ///     The <see cref="IOmadaController"/> that can be used to communicate with a single TP-Link Omada controller.
    /// </returns>
    public IUnauthenticatedOmadaController Create(string omadaUri) =>
        this.Create(new Uri(omadaUri));
}
