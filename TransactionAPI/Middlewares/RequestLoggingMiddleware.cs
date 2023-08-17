namespace TransactionAPI.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            LogRequest(context.Request);

            await _next(context);

            LogResponse(context.Response);
        }

        private void LogRequest(HttpRequest request)
        {
            _logger.LogInformation($"Request: {request.Method} {request.Path}");
        }

        private void LogResponse(HttpResponse response)
        {
            _logger.LogInformation($"Response: {response.StatusCode}");
        }
    }
}
