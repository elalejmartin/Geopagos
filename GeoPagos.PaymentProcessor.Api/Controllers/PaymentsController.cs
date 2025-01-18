using GeoPagos.PaymentProcessor.Application.DTOs;
using GeoPagos.PaymentProcessor.Application.Interfaces;
using GeoPagos.PaymentProcessor.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeoPagos.PaymentProcessor.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IPaymentService _paymentService;   
        public PaymentsController(ILogger<PaymentsController> logger, IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(PaymentDto model)
        {
            try
            {
                var response = _paymentService.VerifyAmount(model);    

                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

        }
    }
}
