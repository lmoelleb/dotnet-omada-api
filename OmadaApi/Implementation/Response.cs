namespace OmadaApi.Implementation;
/// <summary>
///     Represents a JSON response from the Omada API.
/// </summary>
/// <typeparam name="T">
///     The type of the <see cref="Result"/> returned by the Omada API.
/// </typeparam>
internal record Response<T>
{
    /// <summary>
    /// Gets the Omada error code. See the API documentation for possible values.
    /// </summary>
    required public int ErrorCode { get; init; }

    /// <summary>
    /// Gets the message returned by the Omada API if any.
    /// </summary>
    public string Msg { get; init; } = string.Empty;

    /// <summary>
    /// Gets the result of the call if available.
    /// </summary>
    public T? Result { get; init; }
}
