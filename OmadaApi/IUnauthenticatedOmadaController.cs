namespace OmadaApi;

/// <summary>
///     Represents an Omada Controller API that has noot been authenticated.
///     Use <see cref="OmadaControllerFactory"/> to create instances of this interface.
/// </summary>
/// <seealso cref="OmadaControllerFactory"/>
public interface IUnauthenticatedOmadaController : IOmadaController
{
    /// <summary>
    ///     Gets an <see cref="IAuthenticatedOmadaController"/> for the specified username and password.
    /// </summary>
    /// <remarks>
    ///     Authentication will not occure before the first API call so no exception will be thrown by
    ///     this metod if the username and/or password is incorrect.
    /// </remarks>
    /// <param name="username">
    ///     The username to use when authentication against the Omada Controller.
    /// </param>
    /// <param name="password">
    ///     The password to use when authentication against the Omada Controller.
    /// </param>
    /// <returns>
    ///     An <see cref="IAuthenticatedOmadaController"/> that can be used to call the API on the Omada Controller.
    /// </returns>
    IAuthenticatedOmadaController WithCredentials(string username, string password);

    /// <summary>
    ///     Gets an <see cref="IAuthenticatedOmadaController"/> for the specified token.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only use this method of you have retrieved the token yourself. For a normal
    ///         authentication flow, use <see cref="WithCredentials(string, string)"/> instead.
    ///     </para>
    ///     <para>
    ///     Authentication will not occure before the first API call so no exception will be thrown by
    ///     this metod if the token is incorrect.
    ///     </para>
    /// </remarks>
    /// <param name="token">
    ///     The token passed to the Omada Controller with each request.
    /// </param>
    /// <returns>
    ///     An <see cref="IAuthenticatedOmadaController"/> that can be used to call the API on the Omada Controller.
    /// </returns>
    IAuthenticatedOmadaController WithToken(string token);
}
