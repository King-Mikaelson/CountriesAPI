using System.Diagnostics.Metrics;
using CountriesAPI.Data;
using CountriesAPI.Models;
using CountriesAPI.Models.DTOs;
using CountriesAPI.Models.External;
using CountryCurrencyAPI.Services;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace CountriesAPI.Services
{
    public class CountryService : ICountryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IExternalApiService _externalApi;
        private readonly ILogger<CountryService> _logger;
        private readonly IImageService _imageService;

        public CountryService(
            ApplicationDbContext context,
            IExternalApiService externalApi,
           IImageService imageService,
           ILogger<CountryService> logger)
        {
            _context = context;
            _externalApi = externalApi;
            _logger = logger;
            _imageService= imageService;

        }

        /// <summary>
        /// Fetches data from external APIs and refreshes database
        /// </summary>
        public async Task<RefreshResponseDto> RefreshCountriesAsync()
        {
            try
            {
                _logger.LogInformation("Starting country refresh process");

                // Step 1: Fetch data from external APIs
                var fetchCountriesTask = _externalApi.FetchCountriesAsync();
                var fetchExchangeTask = _externalApi.FetchExchangeRatesAsync();
                await Task.WhenAll(fetchCountriesTask, fetchExchangeTask);

                var externalCountries = fetchCountriesTask.Result;
                var exchangeRates = fetchExchangeTask.Result.Rates;

                _logger.LogInformation("Fetched {Count} countries and {RateCount} exchange rates",
                    externalCountries.Count, exchangeRates.Count);

                var refreshTimestamp = DateTime.UtcNow;
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(refreshTimestamp, timeZone);

                var existingCountries = await _context.Countries.ToListAsync();
                var existingCountriesDict = existingCountries.ToDictionary(
                    c => c.name.ToLower(),
                    c => c
                );


                int updatedCount = 0;
                int addedCount = 0;


                foreach (var externalCountry in externalCountries)
                {
                    // Extract currency code (first currency only)
                    string? currencyCode = null;
                    if (externalCountry.Currencies != null && externalCountry.Currencies.Any())
                    {
                        currencyCode = externalCountry.Currencies.First().Code;
                    }

                    // Get exchange rate for this currency
                    decimal? exchangeRate = null;
                    if (!string.IsNullOrEmpty(currencyCode) && exchangeRates.ContainsKey(currencyCode))
                    {
                        exchangeRate = exchangeRates[currencyCode];
                    }

                    // Calculate estimated GDP
                    decimal? estimatedGdp = CalculateEstimatedGdp(
                        externalCountry.Population,
                        exchangeRate);

                    // Check if country exists
                    if (existingCountriesDict.TryGetValue(externalCountry.Name.ToLower(), out var existingCountry))
                    {
                        // UPDATE existing country
                        existingCountry.capital = externalCountry.Capital;
                        existingCountry.region = externalCountry.Region;
                        existingCountry.population = externalCountry.Population;
                        existingCountry.currency_code = currencyCode;
                        existingCountry.exchange_rate = exchangeRate;
                        existingCountry.estimated_gdp = estimatedGdp;
                        existingCountry.flag_url = externalCountry.Flag;
                        existingCountry.last_refreshed_at = refreshTimestamp;

                        updatedCount++;
                    }
                    else
                    {
                        // INSERT new country
                        var newCountry = new Country
                        {
                            name = externalCountry.Name,
                            capital = externalCountry.Capital,
                            region = externalCountry.Region,
                            population = externalCountry.Population,
                            currency_code = currencyCode,
                            exchange_rate = exchangeRate,
                            estimated_gdp = estimatedGdp,
                            flag_url = externalCountry.Flag,
                            last_refreshed_at = refreshTimestamp
                        };

                        _context.Countries.Add(newCountry);
                        addedCount++;
                    }
                }



                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully processed {Total} countries ({Added} added, {Updated} updated)",
                    addedCount + updatedCount, addedCount, updatedCount);


                var allCountries = await _context.Countries.ToListAsync();
                await _imageService.GenerateSummaryImageAsync(allCountries, refreshTimestamp);
                _logger.LogInformation("Summary image generated successfully");
 

                return new RefreshResponseDto
                {
                    total_countries = addedCount + updatedCount,
                    refreshed_at = localTime
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch data from external APIs");
                throw new InvalidOperationException("External data source unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during country refresh");
                throw;
            }
        }

        private Country? MapExternalToCountry(ExternalCountryDto externalCountry,
           Dictionary<string, decimal> exchangeRates, DateTime refreshTimestamp)
        {
            try
            {
                string? currencyCode = externalCountry.Currencies?.FirstOrDefault()?.Code;
                decimal? exchangeRate = !string.IsNullOrEmpty(currencyCode) && exchangeRates.ContainsKey(currencyCode)
                    ? exchangeRates[currencyCode]
                    : null;

                decimal? estimatedGdp = CalculateEstimatedGdp(externalCountry.Population, exchangeRate);

                return new Country
                {
                    name = externalCountry.Name,
                    capital = externalCountry.Capital,
                    region = externalCountry.Region,
                    population = externalCountry.Population,
                    currency_code = currencyCode,
                    exchange_rate = exchangeRate,
                    estimated_gdp = estimatedGdp,
                    flag_url = externalCountry.Flag,
                    last_refreshed_at = refreshTimestamp
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to map country {Name}", externalCountry.Name);
                return null; // skip this country
            }
        }


        /// <summary>
        /// Processes a single country: calculates GDP and saves to database
        /// </summary>
        private async Task<Country> ProcessCountryAsync(
            ExternalCountryDto externalCountry,
            Dictionary<string, decimal> exchangeRates,
            DateTime refreshTimestamp)
        {
            // Extract currency code (first currency only)
            string? currencyCode = null;
            if (externalCountry.Currencies != null && externalCountry.Currencies.Any())
            {
                currencyCode = externalCountry.Currencies.First().Code;
            }

            // Get exchange rate for this currency
            decimal? exchangeRate = null;
            if (!string.IsNullOrEmpty(currencyCode) && exchangeRates.ContainsKey(currencyCode))
            {
                exchangeRate = exchangeRates[currencyCode];
            }

            // Calculate estimated GDP
            decimal? estimatedGdp = CalculateEstimatedGdp(
                externalCountry.Population,
                exchangeRate);

            // Check if country already exists (case-insensitive)
            var existingCountry = await _context.Countries
                .FirstOrDefaultAsync(c => c.name.ToLower() == externalCountry.Name.ToLower());

            if (existingCountry != null)
            {
                // UPDATE existing country
                existingCountry.capital = externalCountry.Capital;
                existingCountry.region = externalCountry.Region;
                existingCountry.population = externalCountry.Population;
                existingCountry.currency_code = currencyCode;
                existingCountry.exchange_rate = exchangeRate;
                existingCountry.estimated_gdp = estimatedGdp;
                existingCountry.flag_url = externalCountry.Flag;
                existingCountry.last_refreshed_at = refreshTimestamp;
                _logger.LogDebug("Updated country: {Name}", externalCountry.Name);
                await _context.SaveChangesAsync();
                return existingCountry;

            }
            else
            {
                // INSERT new country
                var newCountry = new Country
                {
                    name = externalCountry.Name,
                    capital = externalCountry.Capital,
                    region = externalCountry.Region,
                    population = externalCountry.Population,
                    currency_code = currencyCode,
                    exchange_rate = exchangeRate,
                    estimated_gdp = estimatedGdp,
                    flag_url = externalCountry.Flag,
                    last_refreshed_at = refreshTimestamp
                };

                _context.Countries.Add(newCountry);
                _logger.LogDebug("Added new country: {Name}", externalCountry.Name);
                await _context.SaveChangesAsync();
                return newCountry;
            }
        }

        /// <summary>
        /// Calculates estimated GDP using formula: population × random(1000-2000) ÷ exchange_rate
        /// </summary>
        private decimal? CalculateEstimatedGdp(long population, decimal? exchangeRate)
        {
            // If no exchange rate, return null
            if (!exchangeRate.HasValue || exchangeRate.Value == 0)
            {
                return null;
            }

            // Generate random multiplier between 1000 and 2000
            Random random = new Random();
            int multiplier = random.Next(1000, 2001);

            // Calculate GDP
            decimal gdp = (population * multiplier) / exchangeRate.Value;

            return Math.Round(gdp, 2);
        }

        /// <summary>
        /// Gets all countries with optional filters
        /// </summary>
        public async Task<List<CountryResponseDto>> GetAllCountriesAsync(
            string? region = null,
            string? currency = null,
            string? sort = null)
        {
            var query = _context.Countries.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(c => c.region != null && c.region.ToLower() == region.ToLower());
            }

            if (!string.IsNullOrEmpty(currency))
            {
                query = query.Where(c => c.currency_code != null && c.currency_code.ToUpper() == currency.ToUpper());
            }

            // Apply sorting
            query = sort?.ToLower() switch
            {
                "gdp_desc" => query.OrderByDescending(c => c.estimated_gdp),
                "gdp_asc" => query.OrderBy(c => c.estimated_gdp),
                "population_desc" => query.OrderByDescending(c => c.population),
                "population_asc" => query.OrderBy(c => c.population),
                "name_asc" => query.OrderBy(c => c.name),
                "name_desc" => query.OrderByDescending(c => c.name),
                _ => query.OrderBy(c => c.name) // Default sort by name
            };

            var countries = await query.ToListAsync();

            // Map to DTOs
            return countries.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Gets a single country by name (case-insensitive)
        /// </summary>
        public async Task<CountryResponseDto?> GetCountryByNameAsync(string name)
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.name.ToLower() == name.ToLower());

            return country != null ? MapToDto(country) : null;
        }

        /// <summary>
        /// Deletes a country by name (case-insensitive)
        /// </summary>
        public async Task<bool> DeleteCountryAsync(string name)
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.name.ToLower() == name.ToLower());

            if (country == null)
            {
                return false;
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted country: {Name}", name);
            return true;
        }

        /// <summary>
        /// Gets status information (total countries and last refresh time)
        /// </summary>
        public async Task<StatusDto> GetStatusAsync()
        {
            var totalCountries = await _context.Countries.CountAsync();
            var lastRefreshed = await _context.Countries
                .OrderByDescending(c => c.last_refreshed_at)
                .Select(c => c.last_refreshed_at)
                .FirstOrDefaultAsync();

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");

            return new StatusDto
            {
                total_countries = totalCountries,
                last_refreshed_at = totalCountries > 0 ? TimeZoneInfo.ConvertTimeFromUtc(lastRefreshed,timeZone) : null
            };
        }

        /// <summary>
        /// Maps Country entity to CountryResponseDto
        /// </summary>
        private CountryResponseDto MapToDto(Country country)
        {

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");

            return new CountryResponseDto
            {
                id = country.id,
                name = country.name,
                capital = country.capital,
                region = country.region,
                population = country.population,
                currency_code = country.currency_code,
                exchange_rate = country.exchange_rate,
                estimated_gdp = country.estimated_gdp,
                flag_url = country.flag_url,
                last_refreshed_at = TimeZoneInfo.ConvertTimeFromUtc(country.last_refreshed_at,timeZone)
            };
        }
    }
}