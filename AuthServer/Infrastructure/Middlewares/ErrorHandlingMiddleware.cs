using AuthServer.Infrastructure.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(
             HttpContext context,
             Exception exception)
        {
            int statusCode;
            object errors = null;

            switch (exception)
            {
                case RestException restException:
                    {
                        statusCode = (int)restException.Code;

                        if (restException.Message != null & restException.Message is string)
                        {
                            errors = new[] { restException.Message };
                        }

                        break;
                    }

                case ValidationException validationException:
                    {
                        statusCode = 400;

                        string message = validationException.Message
                            .Replace("Validation failed: \r\n -- ", "");

                        if (!string.IsNullOrWhiteSpace(validationException.Message))
                        {
                            string normalizedMessage = message
                                .Replace(" ", "")
                                .Substring(message.IndexOf(":", StringComparison.Ordinal) + 1);

                            errors = new[] { normalizedMessage };
                        }

                        break;
                    }

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    errors = "An internal server error has occured.";
                    break;
            }

            _logger.LogError($"{errors} - {exception.Source} - {exception.Message} - {exception.StackTrace} - {exception.TargetSite.Name}");

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                errors
            }));
        }
    }
}