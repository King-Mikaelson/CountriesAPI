using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StringAnalyzer.Middleware
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // If the model binding failed or model is invalid
            if (!context.ModelState.IsValid)
            {
                // Transform errors into key → array of messages
                var errors = context.ModelState
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                // Return a structured JSON response
                context.Result = new BadRequestObjectResult(new
                {
                    status = 400,
                    error = "BadRequest",
                    message = "One or more validation errors occurred.",
                    errors
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
