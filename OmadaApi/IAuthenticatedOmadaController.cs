namespace OmadaApi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
///     Represents an Omada Controller API that has been authenticated by calling
///     <see cref="IUnauthenticatedOmadaController.WithCredentials(string, string)"/>
///     or <see cref="IUnauthenticatedOmadaController.WithToken(string)"/> on an
///     <see cref="IUnauthenticatedOmadaController"/> created by a
///     <see cref="OmadaControllerFactory"/>.
/// </summary>
public interface IAuthenticatedOmadaController : IOmadaController
{
}
