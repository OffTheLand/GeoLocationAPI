using GeoLocationAPI.V1.Models;
using GeoLocationAPI.V1.Services;
using Microsoft.AspNetCore.Mvc;
namespace GeoLocationAPI.V1.Controllers
{
    /// <summary>
    /// API for retrieving GeoLocations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GeoLocationController : ControllerBase
    {
        private readonly IGeoLocationService _geoLocationService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geoLocationService"></param>
        public GeoLocationController(
            IGeoLocationService geoLocationService
            )

        {
            _geoLocationService = geoLocationService;
        }

        /// <summary>
        ///  Get the GeoLocation for the IPAddress the user specified
        /// </summary>
        /// <response code="200"></response>
        [HttpGet("{IPAddress}")]
        public async Task<ActionResult<GeoLocation>> GetGeoLocationByIPAsync(string IPAddress)
        {
            var items = await _geoLocationService.GetGeoLocationByIPAsync(IPAddress);
            return Ok(items);
        }
    }
}
