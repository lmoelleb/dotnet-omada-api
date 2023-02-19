namespace OmadaApi.Implementation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    internal class UnauthenticatedOmadaController : OmadaController, IUnauthenticatedOmadaController
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnauthenticatedOmadaController"/> class.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="Context"/> to use when calling the Omada controller.
        /// </param>
        public UnauthenticatedOmadaController(Context context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public IAuthenticatedOmadaController WithCredentials(string username, string password)
        {
            return new AuthenticatedOmadaController(this.Context.WithCredentials(username, password));
        }

        /// <inheritdoc />
        public IAuthenticatedOmadaController WithToken(string token)
        {
            return new AuthenticatedOmadaController(this.Context.WithToken(token));
        }
    }
}
