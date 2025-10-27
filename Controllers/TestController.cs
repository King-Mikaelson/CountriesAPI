using CountriesAPI.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CountriesAPI.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IExternalApiService _externalApi;

        public TestController(IExternalApiService externalApi)
        {
            _externalApi = externalApi;
        }

        [HttpGet("countries")]
        public async Task<IActionResult> TestCountries()
        {
            try
            {
                var countries = await _externalApi.FetchCountriesAsync();
                return Ok(new { count = countries.Count, sample = countries });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
        }

        [HttpGet("rates")]
        public async Task<IActionResult> TestRates()
        {
            try
            {
                var rates = await _externalApi.FetchExchangeRatesAsync();
                return Ok(new { count = rates.Rates.Count, sampleRates = rates.Rates });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { error = ex.Message });
            }
        }
    }
}
