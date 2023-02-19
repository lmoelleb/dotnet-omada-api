namespace OmadaApi.Implementation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <inheritdoc/>
internal abstract class OmadaController : IOmadaController
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OmadaController"/> class.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="Context"/> to use when calling the API on the Omada Controller.
    /// </param>
    protected OmadaController(Context context)
    {
        this.Context = context;
    }

    /// <summary>
    /// Gets the <see cref="Context"/> to use when calling the API on the Omada Controller.
    /// </summary>
    protected Context Context { get; }

    /// <inheritdoc/>
    public Task<ControllerInfo> GetControllerInfoAsync(CancellationToken cancellationToken = default) => this.Context.GetControllerInfoAsync();
}
