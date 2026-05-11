using System.Net;

namespace InventorySalesManagementSystem.API.Middleware;

public class AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
