namespace OmadaApi;
internal record LoginRequest
{
    /// <summary>
    /// Gets the username to use when authenticating the user.
    /// </summary>
    required public string Username { get; init; }

    /// <summary>
    /// Gets the password to use when authenticating the user.
    /// </summary>
    required public string Password { get; init; }
}
