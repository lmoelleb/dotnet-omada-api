namespace OmadaApi.Generator.Definition;

using System;
using System.Collections.Generic;
using System.Text;

public record ApiErrorMessage
{
    public ApiErrorMessage(int errorCode, string errorDescription)
    {
        this.ErrorCode = errorCode;
        this.ErrorDescription = errorDescription;
    }

    public int ErrorCode { get; }

    public string ErrorDescription { get; }
}
