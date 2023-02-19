namespace OmadaApi;

/// <summary>
/// Represents the result of calling the /api/info endpoint on the Omada Controller.
/// </summary>
public record ControllerInfo
{
    /* {"controllerVer":"5.7.6","apiVer":"3","configured":true,"type":10,"supportApp":true,"omadacId":"e93352b262296409a40f2ffae0785450"} */

    /// <summary>
    /// Gets the version of the controller, for example 5.7.6.
    /// </summary>
    required public string ControllerVer { get; init; }

    /// <summary>
    /// Gets the API version supported by the controller, for example 3.
    /// </summary>
    required public string ApiVer { get; init; }

    /// <summary>
    /// Gets a value indicating whether the controller has been configured.
    /// </summary>
    required public bool Configured { get; init; }

    /// <summary>
    /// Gets the Type (purpose not known at time of writing).
    /// </summary>
    required public int Type { get; init; }

    /// <summary>
    /// Gets a value indicating whether the controller supports the Omada App.
    /// </summary>
    required public bool SupportApp { get; init; }

    /// <summary>
    /// Gets the Omada Controller Id.
    /// </summary>
    required public string OmadaCId { get; init; }
}
