namespace Countries.Models.DTOs
{
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    public class ValidationErrorResponse
    {
        public string Error { get; set; } = "Validation failed";
        public Dictionary<string, string> Details { get; set; } = new();
    }
}