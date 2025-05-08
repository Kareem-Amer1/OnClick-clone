using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Talabat.Core.Services;

namespace Talabat.APlS.Helpers
{
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _expireTimeInSeconds;

        public CachedAttribute(int ExpireTimeInSeconds)
        {
            _expireTimeInSeconds = ExpireTimeInSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var CacheService = context.HttpContext.RequestServices
                 .GetRequiredService<IResponseCacheService>();
            var CacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            var CachedResponse = await CacheService.GetCachedResponse(CacheKey);
            if (!string.IsNullOrEmpty(CachedResponse))
            {
                var contextResult = new ContentResult()
                {
                    Content = CachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contextResult;
                return;
            }
            var ExecutedEndPointContext = await next.Invoke();
            if (ExecutedEndPointContext.Result is OkObjectResult result)
            {
                
                await CacheService.CacheResponseAsync(CacheKey, result.Value, TimeSpan.FromSeconds(_expireTimeInSeconds));
            }
                //.ContinueWith(task =>
                //{
                //    var CachedResponse = task.Result;
                //    if (!string.IsNullOrEmpty(CachedResponse))
                //    {
                //        context.Result = new ContentResult()
                //        {
                //            Content = CachedResponse,
                //            ContentType = "application/json",
                //            StatusCode = 200
                //        };
                //    }
                //});
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(request.Path);
            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            return keyBuilder.ToString();
        }
    }
}
