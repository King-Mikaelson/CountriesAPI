using System.Text.Json;
using CountriesAPI.Models.DTOs;
using CountriesAPI.Models.External;
namespace CountriesAPI.Services
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalApiService> _logger;

        // API URLs as constants
        private const string CountriesApiUrl = "https://restcountries.com/v2/all?fields=name,capital,region,population,flag,currencies";
        private const string ExchangeRatesApiUrl = "https://open.er-api.com/v6/latest/USD";

        public ExternalApiService(HttpClient httpClient, ILogger<ExternalApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<List<ExternalCountryDto>> FetchCountriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching countries from {Url}", CountriesApiUrl);

                var response = await _httpClient.GetAsync(CountriesApiUrl);

                // Check if request was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch countries. Status code: {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"RestCountries API returned {response.StatusCode}");
                }

                // Read response content
                var jsonString = await response.Content.ReadAsStringAsync();

                // Deserialize JSON to list of countries
                var countries = JsonSerializer.Deserialize<List<ExternalCountryDto>>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Handle case differences
                });

                if (countries == null || countries.Count == 0)
                {
                    _logger.LogWarning("No countries returned from API");
                    return new List<ExternalCountryDto>();
                }

                _logger.LogInformation("Successfully fetched {Count} countries", countries.Count);
                return countries;
            }
            catch (TaskCanceledException ex)
            {
                // Timeout occurred
                _logger.LogError(ex, "Request to RestCountries API timed out");
                throw new HttpRequestException("Could not fetch data from RestCountries API: Request timed out", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching countries");
                throw new HttpRequestException("Could not fetch data from RestCountries API", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse countries JSON response");
                throw new InvalidOperationException("Failed to parse response from RestCountries API", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching countries");
                throw;
            }
        }

        /// <summary>
        /// Fetches exchange rates from Exchange Rate API (base: USD)
        /// </summary>
        public async Task<ExchangeRatesDto> FetchExchangeRatesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching exchange rates from {Url}", ExchangeRatesApiUrl);

                var response = await _httpClient.GetAsync(ExchangeRatesApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch exchange rates. Status code: {StatusCode}", response.StatusCode);
                    throw new HttpRequestException($"Exchange Rate API returned {response.StatusCode}");
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                var exchangeRates = JsonSerializer.Deserialize<ExchangeRatesDto>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (exchangeRates == null || exchangeRates.Rates == null || exchangeRates.Rates.Count == 0)
                {
                    _logger.LogWarning("No exchange rates returned from API");
                    throw new InvalidOperationException("Exchange rates data is empty");
                }

                _logger.LogInformation("Successfully fetched {Count} exchange rates", exchangeRates.Rates.Count);
                return exchangeRates;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to Exchange Rate API timed out");
                throw new HttpRequestException("Could not fetch data from Exchange Rate API: Request timed out", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching exchange rates");
                throw new HttpRequestException("Could not fetch data from Exchange Rate API", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse exchange rates JSON response");
                throw new InvalidOperationException("Failed to parse response from Exchange Rate API", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching exchange rates");
                throw;
            }
        }

    }
}
