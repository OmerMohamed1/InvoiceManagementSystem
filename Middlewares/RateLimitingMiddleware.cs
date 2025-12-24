using DocumentFormat.OpenXml.Presentation;
using System.Collections.Concurrent;

namespace InvoiceManagementSystem.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static int _counter = 0;
        private static DateTime _lastRequsetDate = DateTime.Now;
        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            _counter++;
            if (DateTime.Now.Subtract(_lastRequsetDate).Seconds > 10)
            {
                _counter = 1;
                _lastRequsetDate = DateTime.Now;
                await _next(context);
            }
            else
            {
                if (_counter > 5)
                {
                    _lastRequsetDate = DateTime.Now;
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                }
                else
                {
                    _lastRequsetDate = DateTime.Now;
                    await _next(context);
                }
            }
        }
    }
}
