using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StringAnalyzer.Exceptions;

namespace StringAnalyzer.Middleware
{
    public class Middleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<Middleware> _logger;

        public Middleware(RequestDelegate next, ILogger<Middleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (StringAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, "Duplicate string detected.");
                await HandleExceptionAsync(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (StringNotFoundException ex)
            {
                _logger.LogWarning(ex, "String Not Found.");
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (InvalidStringException ex)
            {
                _logger.LogWarning(ex, "Invalid string request.");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (InvalidStringTypeException ex)
            {
                _logger.LogWarning(ex, "Unprocessable string data.");
                await HandleExceptionAsync(context, HttpStatusCode.UnprocessableEntity, ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "JSON deserialization failed.");
                await HandleExceptionAsync(context, HttpStatusCode.UnprocessableEntity,
                    "Invalid data type or malformed JSON in request body.");
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed.");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected server error.");
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                error = statusCode.ToString(),
                message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
