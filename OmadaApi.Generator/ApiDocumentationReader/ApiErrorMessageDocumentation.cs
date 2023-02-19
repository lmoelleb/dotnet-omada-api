namespace OmadaApi.Generator.ApiDocumentationReader;
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
