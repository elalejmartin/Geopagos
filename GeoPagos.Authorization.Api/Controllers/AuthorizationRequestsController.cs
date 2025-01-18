using GeoPagos.Authorization.Application.DTOs;
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
        private readonly IAuthorizationRequestFactory _authorizationRequestFactory;
        public AuthorizationRequestsController(ILogger<AuthorizationRequestsController> logger, IAuthorizationRequestFactory authorizationRequestFactory)
        {
            _logger = logger;
            _authorizationRequestFactory = authorizationRequestFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Post(AuthorizationRequestDto model)
        {
            try
            {
                var implementation = _authorizationRequestFactory.GetAuthorizationRequest(model.CustomerType);
                await implementation.Authorize(model);
                return Ok("Hello World");
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
