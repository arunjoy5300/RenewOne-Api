namespace RenewOne.Application.Common.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; } // HTTP Status Code (200, 401, 400 etc.)
        public List<string>? Errors { get; set; }

        // 200 - Success
        public static Result<T> Success(T data, string message = "Success", int statusCode = 200)
        {
            return new Result<T> { IsSuccess = true, Data = data, Message = message, StatusCode = statusCode };
        }

        // 400 - General Failure / Validation Error
        public static Result<T> Failure(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new Result<T> { IsSuccess = false, Message = message, StatusCode = statusCode, Errors = errors };
        }

        // 404 - Not Found
        public static Result<T> NotFound(string message = "Resource not found")
        {
            return new Result<T> { IsSuccess = false, Message = message, StatusCode = 404 };
        }

        // 401 - Unauthorized (ലോഗിൻ ഫെയിൽ ആകുമ്പോൾ)
        public static Result<T> Unauthorized(string message = "Unauthorized access")
        {
            return new Result<T> { IsSuccess = false, Message = message, StatusCode = 401 };
        }

        // 409 - Conflict (ഉദാഹരണത്തിന് ഒരേ യൂസർനെയിമിൽ വീണ്ടും രജിസ്റ്റർ ചെയ്യാൻ നോക്കുമ്പോൾ)
        public static Result<T> Conflict(string message = "Conflict occurred")
        {
            return new Result<T> { IsSuccess = false, Message = message, StatusCode = 409 };
        }
    }
}
