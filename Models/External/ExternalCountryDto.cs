namespace CountriesAPI.Models.External;
public class ExternalCountryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Capital { get; set; }
    public string? Region { get; set; }
    public long Population { get; set; }
    public string? Flag { get; set; }

    // List because a country can have multiple currencies
    public List<CurrencyInfo>? Currencies { get; set; }
}

/// <summary>
/// Represents currency information from the API
/// </summary>
public class CurrencyInfo
{
    public string? Code { get; set; }      // Like "USD", "NGN"
    public string? Name { get; set; }      // Like "US Dollar"
    public string? Symbol { get; set; }    // Like "$", "₦"
}