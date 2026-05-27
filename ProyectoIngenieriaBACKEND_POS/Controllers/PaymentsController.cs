using Microsoft.AspNetCore.Mvc;
using ProyectoIngenieriaBACKEND_POS.Services;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {

        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("list")] //  api/controller/list
        public async Task<IActionResult> GetPaymentsInfo()
        {
            try
            {
                var result = await _paymentService.GetPaymentsWithClientInfoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la información de los pagos", error = ex.Message });
            }
        }
    }
}
