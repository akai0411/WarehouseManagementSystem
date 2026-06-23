/// <summary>
/// Represents a standardized error response returned by the API.
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of validation or application errors.
    /// </summary>
    public List<string>? Errors { get; set; }
}