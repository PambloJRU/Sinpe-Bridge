using Microsoft.AspNetCore.Mvc;
using ProyectoIngenieriaBACKEND_POS.Models.Dtos;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly ISmsReceiverService _smsReceiverService;

        public SmsController(ISmsReceiverService smsReceiverService)
        {
            _smsReceiverService = smsReceiverService;
        }

        //AQUI VIENE EL MENSAJE EN "CRUDO" CONSTRUIDO DESDE EL SMS RECEIBER DE KOTLIN
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveSms([FromBody] SmsRequestDTO incomingSms)
        {
            
            if (incomingSms == null || string.IsNullOrWhiteSpace(incomingSms.MessageBody))
            {
                return BadRequest(new { message = "La información del SMS es inválida o está vacía." });
            }

            // Procesar el mensaje a través del servicio - VALIDAR - 
            var result = await _smsReceiverService.ProcessIncomingSmsAsync(incomingSms);

            if (result)
            {
                // Devolvemos un 200 OK para que Kotlin sepa que todo salió bien
                return Ok(new { message = "SMS recibido y procesado correctamente en el servidor." });
            }

            return StatusCode(500, new { message = "Error interno procesando el SMS." });
        }

        [HttpGet("testjson")]
        public async Task<IActionResult> TestJson()
        {
            return Ok(new { mensaje = "hola" });
        }
    }
}
