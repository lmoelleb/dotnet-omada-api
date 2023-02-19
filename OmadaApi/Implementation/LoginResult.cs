namespace OmadaApi.Implementation;

/// <summary>
/// The result from calling {controllerId/api/v{apiVersion}/login.
/// </summary>
internal record LoginResult
{
    /// <summary>
    /// Gets the token proving authentication.
    /// </summary>
    required public string Token { get; init; }
}
