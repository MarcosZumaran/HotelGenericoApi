using System.Text.Json.Serialization;

namespace HotelGenericoApi.DTOs.Response;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalCount { get; set; }

    public ApiResponse() { }

    public ApiResponse(T data, string? message = null)
    {
        Success = true;
        Data = data;
        Message = message;
    }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new(data, message);

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };

    public static ApiResponse<T> Fail(List<string> errors) =>
        new() { Success = false, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public ApiResponse() { }

    public ApiResponse(string? message = null)
    {
        Success = true;
        Message = message;
    }

    public static ApiResponse Ok(string? message = null) =>
        new(message);

    public static ApiResponse Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };

    public static ApiResponse Fail(List<string> errors) =>
        new() { Success = false, Errors = errors };
}
