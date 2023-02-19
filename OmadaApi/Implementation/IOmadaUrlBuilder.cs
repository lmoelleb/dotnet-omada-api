namespace OmadaApi.Implementation;

using System;

/// <summary>
/// An immutable builder used to contruct URLs to call the Omada API.
/// </summary>
/// <remarks>
/// Implemented internally instead of using Flurl to keep dependencies down.
/// </remarks>
internal interface IOmadaUrlBuilder
{
    /// <summary>
    ///     Gets a value indicating whether the Omada Controller Id is required for the call.
    /// </summary>
    bool RequiresControllerId { get; }

    /// <summary>
    ///     Creates a new <see cref="IOmadaUrlBuilder"/> with the value of a path parameter specified.
    /// </summary>
    /// <param name="parameterName">
    ///     The name of the path parameter getting specified.
    /// </param>
    /// <param name="value">
    ///     The value of the path parameter getting specified.
    /// </param>
    /// <returns>
    ///     A new <see cref="IOmadaUrlBuilder"/> with the value of the path parameter specified.
    /// </returns>
    IOmadaUrlBuilder WithPathParameter(string parameterName, string value);

    /// <summary>
    ///     Creates a new <see cref="IOmadaUrlBuilder"/> with the value of a query parameter specified.
    /// </summary>
    /// <param name="parameterName">
    ///     The name of the query parameter getting specified.
    /// </param>
    /// <param name="value">
    ///     The value of the query parameter getting specified.
    /// </param>
    /// <returns>
    ///     A new <see cref="IOmadaUrlBuilder"/> with the value of the query parameter specified.
    /// </returns>
    IOmadaUrlBuilder WithQueryParameter(string parameterName, string value);

    /// <summary>
    ///     Constructs a <see cref="Uri"/> with the value defined by this <see cref="IOmadaUrlBuilder"/>.
    /// </summary>
    /// <returns>
    ///     A <see cref="Uri"/> with the value defined by this <see cref="IOmadaUrlBuilder"/>.
    /// </returns>
    Uri Build();
}
