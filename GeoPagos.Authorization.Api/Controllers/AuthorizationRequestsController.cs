using GeoPagos.Authorization.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeoPagos.Authorization.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationRequestsController : ControllerBase
    {
        private readonly ILogger<AuthorizationRequestsController> _logger;
        private readonly IAuthorizationRequestService _authorizationRequestService;
        public AuthorizationRequestsController(ILogger<AuthorizationRequestsController> logger, IAuthorizationRequestService authorizationRequestService)
        {
            _logger = logger;
            _authorizationRequestService = authorizationRequestService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }
    }
}
