using Countries.Models.DTOs;
using CountriesAPI.Models.DTOs;
using CountriesAPI.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("countries")]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly IImageService _imageService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(
        ICountryService countryService,
        IImageService imageService,
        ILogger<CountriesController> logger)
    {
        _countryService = countryService;
        _imageService = imageService;
        _logger = logger;
    }



    // POST /countries/refresh
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 503)]
    public async Task<IActionResult> RefreshCountries()
    {
        try
        {
            _logger.LogInformation("Refresh endpoint called");

            var result = await _countryService.RefreshCountriesAsync();

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("External data source unavailable"))
        {
            _logger.LogError(ex, "External API unavailable during refresh");

            // Determine which API failed
            string apiName = ex.InnerException?.Message.Contains("RestCountries") == true
                ? "RestCountries API"
                : "Exchange Rate API";

            return StatusCode(503, new ErrorResponse
            {
                Error = "External data source unavailable",
                Details = $"Could not fetch data from {apiName}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during refresh");
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error"
            });
        }
    }


    // GET /countries
    [HttpGet]
    [ProducesResponseType(typeof(List<CountryResponseDto>), 200)]
    public async Task<IActionResult> GetAllCountries(
        [FromQuery] string? region = null,
        [FromQuery] string? currency = null,
        [FromQuery] string? sort = null)
    {
        try
        {
            _logger.LogInformation("Getting all countries with filters - Region: {Region}, Currency: {Currency}, Sort: {Sort}",
                region, currency, sort);

            var countries = await _countryService.GetAllCountriesAsync(region, currency, sort);

            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting countries");
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error"
            });
        }
    }


    [HttpGet("{name}")]
    [ProducesResponseType(typeof(CountryResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> GetCountryByName(string name)
    {
        try
        {
            _logger.LogInformation("Getting country by name: {Name}", name);

            var country = await _countryService.GetCountryByNameAsync(name);

            if (country == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Country not found"
                });
            }

            return Ok(country);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting country: {Name}", name);
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error"
            });
        }
    }


    [HttpDelete("{name}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<IActionResult> DeleteCountry(string name)
    {
        try
        {
            _logger.LogInformation("Deleting country: {Name}", name);

            var success = await _countryService.DeleteCountryAsync(name);

            if (!success)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Country not found"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting country: {Name}", name);
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error"
            });
        }
    }

    [HttpGet("/status")]
    [ProducesResponseType(typeof(StatusDto), 200)]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            _logger.LogInformation("Getting status");

            var status = await _countryService.GetStatusAsync();

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status");
            return StatusCode(500, new ErrorResponse
            {
                Error = "Internal server error"
            });
        }
    }



[HttpGet("image")]
    public IActionResult GetSummaryImage()
    {
        try
        {
            var imagePath = _imageService.GetSummaryImagePath();
            
            if (imagePath == null)
            {
                return NotFound(new { error = "Summary image not found" });
            }

            // Return the image file
            var imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return File(imageBytes, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving summary image");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}