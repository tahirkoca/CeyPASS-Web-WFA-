namespace CeyPASS.Models
{
    /// <summary>API yanıt sarmalayıcısı; başarı, mesaj ve isteğe bağlı veri taşır.</summary>
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        /// <summary>HTTP benzeri hata kodu; Failure varsayılanı 500.</summary>
        public int ErrorCode { get; set; }

        /// <returns>Başarılı sonuç.</returns>
        public static ApiResult<T> Ok(T data, string? message = null)
        {
            return new ApiResult<T> { Success = true, Data = data, Message = message };
        }

        /// <param name="errorCode">İstemci/ sunucu hata kodu.</param>
        /// <returns>Başarısız sonuç.</returns>
        public static ApiResult<T> Failure(string message, int errorCode = 500)
        {
            return new ApiResult<T> { Success = false, Message = message, ErrorCode = errorCode };
        }
    }

    /// <summary>Veri gövdesi olmayan API yanıtları için.</summary>
    public class ApiResult : ApiResult<object>
    {
        /// <returns>Başarılı sonuç.</returns>
        public static ApiResult Ok(string? message = null)
        {
            return new ApiResult { Success = true, Message = message };
        }

        /// <returns>Başarısız sonuç.</returns>
        public static new ApiResult Failure(string message, int errorCode = 500)
        {
            return new ApiResult { Success = false, Message = message, ErrorCode = errorCode };
        }
    }
}
