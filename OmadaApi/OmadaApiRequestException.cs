namespace OmadaApi;

using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// Thrown when the OmadaApi returns an error code.
/// If an error occure without an error code available an
/// <see cref="HttpRequestException"/> will be thrown instead.
/// </summary>
[Serializable]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Until the bloody analyzer tells me WHAT it detected to be wrong, I am going to ignore it.")]
public partial class OmadaApiRequestException : HttpRequestException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OmadaApiRequestException"/> class representing an error response from the Omada Controller.
    /// </summary>
    /// <param name="httpMethod">The <see cref="HttpMethod"/> used when calling the Omada Controller.</param>
    /// <param name="httpStatusCode">The <see cref="HttpStatusCode"/> received from the Omada Controller.</param>
    /// <param name="omadaErrorCode">The Omada API specific error code received from the Omada Controller.</param>
    /// <param name="omadaErrorMessage">The error message (if any) received from the Omada controller.</param>
    /// <param name="url">The URL that was accessed on the Omada controller.</param>
    /// <param name="innerException">The <see cref="Exception"/>.<see cref="Exception.InnerException"/> if available.</param>
    internal OmadaApiRequestException(
        HttpMethod httpMethod,
        HttpStatusCode httpStatusCode,
        int omadaErrorCode,
        string? omadaErrorMessage,
        string url,
        Exception? innerException = null)
        : base(GetMessage(httpMethod, httpStatusCode, omadaErrorCode, omadaErrorMessage, url), innerException, httpStatusCode)
    {
        this.HttpMethod = httpMethod;
        this.OmadaErrorCode = omadaErrorCode;
        this.OmadaErrorMessage = omadaErrorMessage ?? string.Empty;
        this.Url = MaskTokenRegex().Replace(url, m => m.Groups["prefix"].Value + "***MASKED***");
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OmadaApiRequestException"/> class.
    /// </summary>
    /// <param name="serializationInfo">
    ///     The <see cref="SerializationInfo "/> containing the properties of the serialized exeption.
    /// </param>
    /// <param name="streamingContext">
    ///     The <see cref="StreamingContext"/> of the deserialization.
    /// </param>
    /// <exception cref="NotSupportedException">
    ///     Thrown if the <see cref="SerializationInfo"/> provided are missing mandatory values.
    /// </exception>
    protected OmadaApiRequestException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
        this.HttpMethod = new HttpMethod(serializationInfo.GetString($"{typeof(OmadaApiRequestException).FullName}{nameof(this.HttpMethod)}") ?? throw new NotSupportedException($"Unable to deserialize the exception due to missing value for {nameof(this.HttpMethod)}"));
        this.OmadaErrorCode = serializationInfo.GetInt32($"{typeof(OmadaApiRequestException).FullName}{nameof(this.OmadaErrorCode)}");
        this.OmadaErrorMessage = serializationInfo.GetString($"{typeof(OmadaApiRequestException).FullName}{nameof(this.OmadaErrorMessage)}") ?? string.Empty;
        this.Url = serializationInfo.GetString($"{typeof(OmadaApiRequestException).FullName}{nameof(this.Url)}") ?? string.Empty;
    }

    /// <summary>
    /// Gets the <see cref="HttpMethod"/> used when calling the Omada API.
    /// </summary>
    public HttpMethod HttpMethod { get; }

    /// <summary>
    /// Gets the Omada API specific error codes.
    /// See the Omada API documentation for a list of error codes for each available endpoint.
    /// </summary>
    public int OmadaErrorCode { get; }

    /// <summary>
    /// Gets the error message returned by the Omada API. This might be an empty string.
    /// </summary>
    public string OmadaErrorMessage { get; }

    /// <summary>
    /// Gets the URL that was called when the message failed.
    /// </summary>
    public string Url { get; }

    /// <summary>
    ///     Serialize the exception properties to a <see cref="SerializationInfo"/>.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="SerializationInfo"/> the properties should be serialized to.
    /// </param>
    /// <param name="context">
    ///     The <see cref="StreamingContext"/> of the serialization.
    /// </param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue($"{typeof(OmadaApiRequestException).FullName}{nameof(this.HttpMethod)}", this.HttpMethod.ToString());
        info.AddValue($"{typeof(OmadaApiRequestException).FullName}{nameof(this.Url)}", this.Url);
        info.AddValue($"{typeof(OmadaApiRequestException).FullName}{nameof(this.OmadaErrorCode)}", this.OmadaErrorCode);
        info.AddValue($"{typeof(OmadaApiRequestException).FullName}{nameof(this.OmadaErrorMessage)}", this.OmadaErrorMessage);
        base.GetObjectData(info, context);
    }

    [GeneratedRegex(@"(?'prefix'\?(.*&)?token=)[^&]+", RegexOptions.None, "en-US")]
    private static partial Regex MaskTokenRegex();

    private static string GetMessage(HttpMethod httpMethod, HttpStatusCode httpStatusCode, int omadaErrorCode, string? omadaErrorMessage, string url)
    {
        string msgPart = string.IsNullOrEmpty(omadaErrorMessage) ?
                ". No error message was returned by the Omada API" :
                ": " + omadaErrorMessage;
        return $"Calling {httpMethod} {url} failed with HTTP status code {httpStatusCode} and Omada API error code {omadaErrorCode}{msgPart}";
    }
}
