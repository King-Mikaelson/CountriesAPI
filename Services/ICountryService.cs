using CountriesAPI.Models;
using CountriesAPI.Models.DTOs;

namespace CountriesAPI.Services;

public interface ICountryService
{
    /// <summary>
    /// Fetches data from external APIs and refreshes database
    /// </summary>
    Task<RefreshResponseDto> RefreshCountriesAsync();

    /// <summary>
    /// Gets all countries with optional filters
    /// </summary>
    Task<List<CountryResponseDto>> GetAllCountriesAsync(
        string? region = null,
        string? currency = null,
        string? sort = null);

    /// <summary>
    /// Gets a single country by name
    /// </summary>
    Task<CountryResponseDto?> GetCountryByNameAsync(string name);

    /// <summary>
    /// Deletes a country by name
    /// </summary>
    Task<bool> DeleteCountryAsync(string name);

    /// <summary>
    /// Gets status information
    /// </summary>
    Task<StatusDto> GetStatusAsync();
}