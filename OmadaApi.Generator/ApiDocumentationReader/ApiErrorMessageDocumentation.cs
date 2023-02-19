namespace OmadaApi.Generator.ApiDocumentationReader;

using System;
using System.Collections.Generic;
using System.Text;

public record ApiErrorMessageDocumentation
{
    public ApiErrorMessageDocumentation(int errorCode, string errorDescription)
    {
        this.ErrorCode = errorCode;
        this.ErrorDescription = errorDescription;
    }

    public int ErrorCode { get; }

    public string ErrorDescription { get; }
}
