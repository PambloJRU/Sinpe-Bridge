using Microsoft.AspNetCore.Mvc;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Controllers
{
    [ApiController]
    [Route("api/phone")]
    public class PhoneConnectionController : ControllerBase
    {
        private readonly IPhoneConnectionService _phoneConnectionService;

        public PhoneConnectionController(IPhoneConnectionService phoneConnectionService)
        {
            _phoneConnectionService = phoneConnectionService;
        }

        [HttpPost("heartbeat")]
        public async Task<IActionResult> Heartbeat([FromBody] PhoneHeartbeatRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "La solicitud es invalida." });
            }

            if (string.IsNullOrWhiteSpace(request.DeviceId))
            {
                return BadRequest(new { message = "El deviceId es obligatorio." });
            }

            if (request.DeviceId.Length > 200)
            {
                return BadRequest(new { message = "El deviceId es demasiado largo." });
            }

            if (request.SentAtUtc == default)
            {
                return BadRequest(new { message = "La fecha enviada es invalida." });
            }

            var nowUtc = DateTime.UtcNow;
            if (request.SentAtUtc > nowUtc.AddMinutes(5))
            {
                return BadRequest(new { message = "La fecha enviada es invalida." });
            }

            try
            {
                var status = await _phoneConnectionService.RegisterHeartbeatAsync(request);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _phoneConnectionService.GetStatusAsync();
            return Ok(status);
        }
    }
}
