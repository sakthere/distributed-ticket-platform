using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            return Ok("Auth Done");
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public IActionResult AdminOnly()
        {
            return Ok("Admin access confirmed");
        }
    }
}
