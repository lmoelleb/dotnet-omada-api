namespace OmadaApi.Implementation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <inheritdoc/>
internal class AuthenticatedOmadaController : OmadaController, IAuthenticatedOmadaController
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthenticatedOmadaController"/> class.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="Context"/> to use when calling the API.
    /// </param>
    public AuthenticatedOmadaController(Context context)
        : base(context)
    {
    }
}
