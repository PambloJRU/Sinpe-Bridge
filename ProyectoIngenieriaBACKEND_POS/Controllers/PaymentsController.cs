using Microsoft.AspNetCore.Mvc;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
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

        [HttpGet("pending-review")]
        public async Task<IActionResult> GetPendingReviewPayments()
        {
            try
            {
                var payments = await _paymentService.GetPendingReviewPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{paymentId}/review")]
        public async Task<IActionResult> ReviewPayment(int paymentId, [FromBody] ReviewPaymentDTO dto)
        {
            try
            {
                await _paymentService.ReviewPaymentAsync(paymentId, dto.Approved);
                return Ok(new { message = dto.Approved ? "Pago aprobado" : "Pago rechazado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
    }
}
