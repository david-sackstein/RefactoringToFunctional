using Newtonsoft.Json;

namespace SuperMarket.Service
{
    static class Response
    {
        public static HttpResponse Ok()
        {
            return new HttpResponse { ResponseCode = ResponseCode.Ok };
        }

        public static HttpResponse Ok<T>(T value)
        {
            return new HttpResponse
            {
                ResponseCode = ResponseCode.Ok,
                JsonValue = JsonConvert.SerializeObject(value)
            };
        }

        public static HttpResponse BadRequest(string errorMessage)
        {
            return new HttpResponse
            {
                ResponseCode = ResponseCode.BadRequest,
                JsonValue = JsonConvert.SerializeObject(new { Result = errorMessage })
            };
        }

        public static HttpResponse InternalError(string errorMessage)
        {
            return new HttpResponse
            {
                ResponseCode = ResponseCode.InternalError,
                JsonValue = JsonConvert.SerializeObject(new { Result = errorMessage })
            };
        }
    }
}