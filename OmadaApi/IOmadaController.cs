namespace OmadaApi
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents an Omada Controller API. Any instance of <see cref="IOmadaController"/>
    ///     will either implement <see cref="IAuthenticatedOmadaController"/> or
    ///     <see cref="IUnauthenticatedOmadaController"/>.
    /// </summary>
    /// <seealso cref="OmadaControllerFactory"/>
    /// <seealso cref="IAuthenticatedOmadaController"/>
    /// <seealso cref="IUnauthenticatedOmadaController"/>
    public interface IOmadaController
    {
        /// <summary>
        ///     Gets <see cref="ControllerInfo"/> with basic information about the controller.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> that can be used to request the
        ///     operation is cancelled before completion.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="ControllerInfo"/> with basic controller information.
        /// </returns>
        public Task<ControllerInfo> GetControllerInfoAsync(CancellationToken cancellationToken = default);
    }
}
