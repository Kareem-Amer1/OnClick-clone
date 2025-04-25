using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Talabat.APlS.Errors;

namespace Talabat.APlS.Middlewares
{
    public class ExceptionMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleWare> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleWare(RequestDelegate Next , ILogger<ExceptionMiddleWare> logger,IHostEnvironment env) {
            _next = Next;
            this._logger = logger;
            this._env = env;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //if(_env.IsDevelopment())
                //{
                //    var Response =new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message,ex.StackTrace.ToString());
                //}
                //else
                //{
                //    var Response = new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);
                //}

                var Response=_env.IsDevelopment()
                    ? new ApiExceptionResponse((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace.ToString())
                    : new ApiExceptionResponse((int)HttpStatusCode.InternalServerError);
                var Options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    //WriteIndented = true
                };
                var JsonResponse = JsonSerializer.Serialize(Response, Options);
                await context.Response.WriteAsync(JsonResponse);
            }
        }
    }
}
