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

        // AQUÍ VIENE EL MENSAJE EN "CRUDO" CONSTRUIDO DESDE EL SMS RECEIVER DE KOTLIN
        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveSms([FromBody] SmsRequestDTO incomingSms)
        {
            Console.WriteLine($"[SMS ENDPOINT] Received request");
            Console.WriteLine($"[SMS ENDPOINT] SenderNumber: {incomingSms?.SenderNumber ?? "NULL"}");
            Console.WriteLine($"[SMS ENDPOINT] MessageBody: {incomingSms?.MessageBody ?? "NULL"}");
            Console.WriteLine($"[SMS ENDPOINT] ReceivedAt: {incomingSms?.ReceivedAt ?? default}");

            if (incomingSms == null)
            {
                Console.WriteLine("[SMS ENDPOINT] Error: incomingSms is null");
                return BadRequest(new { message = "La información del SMS es inválida o está vacía." });
            }
            if (string.IsNullOrWhiteSpace(incomingSms.SenderNumber))
            {
                Console.WriteLine("[SMS ENDPOINT] Error: SenderNumber is empty");
                return BadRequest(new { message = "El número de origen del SMS es inválido o está vacío." });
            }
            if (string.IsNullOrWhiteSpace(incomingSms.MessageBody))
            {
                Console.WriteLine("[SMS ENDPOINT] Error: MessageBody is empty");
                return BadRequest(new { message = "El contenido del SMS es inválido o está vacío." });
            }
            if (incomingSms.ReceivedAt == default)
            {
                Console.WriteLine("[SMS ENDPOINT] Error: ReceivedAt is default");
                return BadRequest(new { message = "La fecha de recepción del SMS es inválida." });
            }

            try
            {
                var result = await _smsReceiverService.ProcessIncomingSmsAsync(incomingSms);
                if (result == null)
                {
                    Console.WriteLine("[SMS ENDPOINT] Error: result is null");
                    return BadRequest(new { message = "El SMS no coincide con el formato esperado." });
                }
                return Ok(new
                {
                    message = "SMS recibido y procesado correctamente.",
                    data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_REFERENCE")
            {
                Console.WriteLine("[SMS ENDPOINT] Error: DUPLICATE_REFERENCE");
                return Conflict(new
                {
                    message = "Error: Este número de referencia ya fue registrado previamente."
                });
            }
            catch (InvalidOperationException ex) when (ex.Message == "PAYMENT_EXPIRED")
            {
                Console.WriteLine("[SMS ENDPOINT] Error: PAYMENT_EXPIRED");
                return BadRequest(new
                {
                    message = "Pago rechazado. Han pasado más de 15 minutos desde que fue realizado."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMS ENDPOINT] Error: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("testjson")]
        public IActionResult TestJson()
        {
            return Ok(new { mensaje = "hola" });
        }
    }
}