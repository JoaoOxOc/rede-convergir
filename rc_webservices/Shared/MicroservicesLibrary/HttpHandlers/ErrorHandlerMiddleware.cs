using MicroservicesLibrary.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroservicesLibrary.HttpHandlers
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly GreetingOptions options;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                var innerEx = error.InnerException;

                switch (error)
                {
                    case AppException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case AuthException e:
                        // custom authentication error
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        break;
                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonSerializer.Serialize(new { message = error?.Message, details = error?.StackTrace, innerException = innerEx?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
