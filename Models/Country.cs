using System.ComponentModel.DataAnnotations;

namespace CountriesAPI.Models;
public class Country
{
    // Primary Key - The database will auto-generate this ID for each country
    [Key]
    public int id { get; set; }

    // Required fields - these MUST have values
    [Required]
    public string name { get; set; } = string.Empty;  // = string.Empty prevents null warnings
    [Required]
    public long population { get; set; }

    // Optional fields - these can be null (that's what ? means)
    public string? capital { get; set; }
    public string? region { get; set; }
    public string? currency_code { get; set; }

    // Decimal is better than float/double for money (more precise)
    public decimal? exchange_rate { get; set; }
    public decimal? estimated_gdp { get; set; }

    public string? flag_url { get; set; }

    // DateTime to track when we last updated this country's data
    public DateTime last_refreshed_at { get; set; } =  DateTime.UtcNow;
}