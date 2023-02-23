namespace OmadaApi.Generator.ApiDefinition;

/// <summary>
///     Represents the permissions required for an API call to be available.
/// </summary>
/// <remarks>
///     Simplified "world view", in reality there are more permission levels,
///     but the documentation is incomplete and using inconsitent terms, so
///     will take a while to sort it out.
/// </remarks>
public enum PermissionLevel
{
    /// <summary>
    ///     Accessible without authentication.
    /// </summary>
    All,

    /// <summary>
    ///     Accessible with reader permissions.
    /// </summary>
    Read,

    /// <summary>
    ///     Accessible with admin permissions.
    /// </summary>
    Admin,
}
