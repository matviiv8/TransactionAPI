using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace TransactionAPI.Middlewares
{
    public class CachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public CachingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            this._next = next;
            this._cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            var cacheKey = GenerateCacheKey(context.Request);

            if (_cache.TryGetValue(cacheKey, out object cachedResponse))
            {
                await ReturnCachedResponse(context, cachedResponse);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                var response = await FormatResponse(context.Response);
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(20));

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task ReturnCachedResponse(HttpContext context, object cachedResponse)
        {
            var responseString = cachedResponse as string;
            if (responseString != null)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseString);
            }
        }

        private string GenerateCacheKey(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(request.Path);
            keyBuilder.Append(request.QueryString);

            return keyBuilder.ToString();
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return responseBody;
        }
    }
}
