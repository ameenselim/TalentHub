using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TalentHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        public IActionResult RegisterCandidate()
        {
            return Ok();
        }
    }
}
