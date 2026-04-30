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
            
            if (incomingSms == null)
            {
                return BadRequest(new { message = "La información del SMS es inválida o está vacía." });
            }

            // Procesar el mensaje a través del servicio - VALIDAR - 
            if (string.IsNullOrWhiteSpace(incomingSms.SenderNumber))
            {
                return BadRequest(new { message = "El número de origen del SMS es inválido o está vacío." });
            }

            if (string.IsNullOrWhiteSpace(incomingSms.MessageBody))
            {
                return BadRequest(new { message = "El contenido del SMS es inválido o está vacío." });
            }

            if (incomingSms.ReceivedAt == default)
            {
                return BadRequest(new { message = "La fecha de recepción del SMS es inválida." });
            }

            try
            {
                var result = await _smsReceiverService.ProcessIncomingSmsAsync(incomingSms);

                if (result == null)
                {
                    return BadRequest(new { message = "El SMS no coincide con el formato esperado." });
                }

                return Ok(new { message = "SMS recibido y procesado correctamente.", data = result });
            }
            catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_REFERENCE")
            {
                // se devuelve 409 Conflict
                return Conflict(new { message = "Error: Este número de referencia ya fue registrado previamente." });
            }
        }

        [HttpGet("testjson")]
        public async Task<IActionResult> TestJson()
        {
            return Ok(new { mensaje = "hola" });
        }
    }
}
