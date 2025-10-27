using CountriesAPI.Models.DTOs;

namespace CountriesAPI.Models.DTOs;

public class CountryResponseDto
{
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public string? capital { get; set; }
    public string? region { get; set; }
    public long population { get; set; }
    public string? currency_code { get; set; }
    public decimal? exchange_rate { get; set; }
    public decimal? estimated_gdp { get; set; }
    public string? flag_url { get; set; }
    public DateTime last_refreshed_at { get; set; }
}

/// <summary>
/// Response for the /status endpoint
/// </summary>
public class StatusDto
{
    public int total_countries { get; set; }
    public DateTime? last_refreshed_at { get; set; }
}


public class RefreshResponseDto
{
    public int total_countries { get; set; }
    public DateTime refreshed_at { get; set; }
}

/// <summary>
/// Standard error response structure
/// </summary>