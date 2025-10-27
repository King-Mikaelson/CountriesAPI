using CountriesAPI.Models.External;

namespace CountriesAPI.Services
{
    public interface IExternalApiService
    {
        /// <summary>
        /// Fetches all countries from RestCountries API
        /// </summary>
        Task<List<ExternalCountryDto>> FetchCountriesAsync();

        /// <summary>
        /// Fetches exchange rates from Exchange Rate API (base: USD)
        /// </summary>
        Task<ExchangeRatesDto> FetchExchangeRatesAsync();
    }
}

