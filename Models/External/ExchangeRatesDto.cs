namespace CountriesAPI.Models.External;

/// <summary>
/// This matches the JSON structure from open.er-api.com
/// </summary>
public class ExchangeRatesDto
{
    public string Result { get; set; } = string.Empty;  // "success" or "error"
    public string Base_Code { get; set; } = string.Empty;  // "USD"

    // Dictionary is like a phone book - look up currency code, get exchange rate
    // Example: Rates["NGN"] = 1600.50
    public Dictionary<string, decimal> Rates { get; set; } = new();
}