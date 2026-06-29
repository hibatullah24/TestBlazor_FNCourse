namespace TestBlazor_FNCourse.Data.Services
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }

        // ✅ Must return ServiceResult<T> not a hardcoded type
        public static ServiceResult<T> Ok(T data, string message = "Success")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }

        public static ServiceResult<T> NotFound(string message = "Not found.")
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default,
                Message = message,
                StatusCode = 404
            };
        }

        public static ServiceResult<T> BadRequest(string message = "Bad request.")
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default,
                Message = message,
                StatusCode = 400
            };
        }

        public static ServiceResult<T> Unauthorized(string message = "Unauthorized.")
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default,
                Message = message,
                StatusCode = 401
            };
        }
    }
}