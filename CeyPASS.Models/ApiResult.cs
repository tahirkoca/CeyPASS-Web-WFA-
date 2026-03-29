namespace CeyPASS.Models
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public int ErrorCode { get; set; }

        public static ApiResult<T> Ok(T data, string? message = null)
        {
            return new ApiResult<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResult<T> Failure(string message, int errorCode = 500)
        {
            return new ApiResult<T> { Success = false, Message = message, ErrorCode = errorCode };
        }
    }

    public class ApiResult : ApiResult<object>
    {
        public static ApiResult Ok(string? message = null)
        {
            return new ApiResult { Success = true, Message = message };
        }

        public static new ApiResult Failure(string message, int errorCode = 500)
        {
            return new ApiResult { Success = false, Message = message, ErrorCode = errorCode };
        }
    }
}
