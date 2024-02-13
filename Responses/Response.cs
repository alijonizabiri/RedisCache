using System.Net;

namespace RedisCacheDemo.Responses;

public class Response<T>
{
    public int StatusCode { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public Response(T data)
    {
        StatusCode = 200;
        Data = data;
    }

    public Response(HttpStatusCode statusCode, string message)
    {
        StatusCode = (int)statusCode;
        Errors.Add(message);
    }
    
    public Response(HttpStatusCode statusCode, List<string> messages)
    {
        StatusCode = (int)statusCode;
        Errors = messages;
    }
}
