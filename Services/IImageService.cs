using CountriesAPI.Models;

namespace CountriesAPI.Services
{
    public interface IImageService
    {
        void GenerateSummaryImage(List<Country> countries, DateTime lastRefreshedAt);
        Task GenerateSummaryImageAsync(List<Country> countries, DateTime lastRefreshedAt);
        string? GetSummaryImagePath();
    }
}