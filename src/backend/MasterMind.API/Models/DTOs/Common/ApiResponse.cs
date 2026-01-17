namespace MasterMind.API.Models.DTOs.Common;

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error details (if any)
    /// </summary>
    public ErrorDetails? Error { get; set; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Create a failed response
    /// </summary>
    public static ApiResponse<T> Fail(string message, string? errorCode = null, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails
            {
                Code = errorCode ?? "ERROR",
                ValidationErrors = validationErrors
            }
        };
    }
}

/// <summary>
/// Non-generic API response for operations without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Create a successful response without data
    /// </summary>
    public static ApiResponse Ok(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Create a failed response without data
    /// </summary>
    public new static ApiResponse Fail(string message, string? errorCode = null, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Error = new ErrorDetails
            {
                Code = errorCode ?? "ERROR",
                ValidationErrors = validationErrors
            }
        };
    }
}

/// <summary>
/// Error details for API responses
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string Code { get; set; } = "ERROR";

    /// <summary>
    /// Validation errors (field -> error messages)
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Stack trace (only in development)
    /// </summary>
    public string? StackTrace { get; set; }
}
