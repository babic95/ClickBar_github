using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Print([FromBody] PrintRequest request)
        {
            using (var httpClient = new HttpClient())
            {
                // IP adresa tableta
                var tabletApiUrl = "http://IP_ADRESA_TABLETA/api/posprint";
                var response = await httpClient.PostAsJsonAsync(tabletApiUrl, request);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "Print job started" });
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new { message = "Failed to start print job" });
                }
            }
        }
    }

    public class PrintRequest
    {
        public string Content { get; set; }
    }
}