using Microsoft.AspNetCore.Mvc;
using ProyectoIngenieriaBACKEND_POS.Models.Enums;
using ProyectoIngenieriaBACKEND_POS.Services.Interfaces;

namespace ProyectoIngenieriaBACKEND_POS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IWebHostEnvironment _env;

        public AuditLogController(IAuditLogService auditLogService, IWebHostEnvironment env)
        {
            _auditLogService = auditLogService;
            _env = env;
        }

        // ────────────────────────────────────────────────────────────────────
        // Consultas a la base de datos
        // ────────────────────────────────────────────────────────────────────

        // GET: api/auditlog
        /// <summary>Retorna todos los eventos de auditoría registrados, del más reciente al más antiguo.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogService.GetAllLogsAsync();
            return Ok(logs);
        }

        // GET: api/auditlog/byevent?eventType=DuplicateReference
        /// <summary>Filtra los logs por tipo de evento. Ejemplo: ?eventType=3 (DuplicateReference)</summary>
        [HttpGet("byevent")]
        public async Task<IActionResult> GetByEventType([FromQuery] EventType eventType)
        {
            try
            {
                var logs = await _auditLogService.GetLogsByEventTypeAsync(eventType);
                return Ok(logs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/auditlog/byrisk?riskLevel=High
        /// <summary>Filtra los logs por nivel de riesgo. Ejemplo: ?riskLevel=3 (High)</summary>
        [HttpGet("byrisk")]
        public async Task<IActionResult> GetByRiskLevel([FromQuery] RiskLevel riskLevel)
        {
            try
            {
                var logs = await _auditLogService.GetLogsByRiskLevelAsync(riskLevel);
                return Ok(logs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Descarga de archivos de auditoría .txt (RF-12)
        // ────────────────────────────────────────────────────────────────────

        // GET: api/auditlog/archivo
        // GET: api/auditlog/archivo?fecha=2026-06-13
        /// <summary>
        /// Devuelve el archivo .txt de auditoría de un día específico para descarga.
        /// Si no se indica fecha, retorna el archivo del día actual (UTC).
        /// Los archivos están en: Logs/Auditoria/auditoria-YYYY-MM-DD.txt
        /// </summary>
        [HttpGet("archivo")]
        public IActionResult DescargarArchivoAuditoria([FromQuery] string? fecha = null)
        {
            // Determinar la fecha: parámetro o hoy
            string fechaStr;

            if (string.IsNullOrWhiteSpace(fecha))
            {
                fechaStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
            }
            else
            {
                // Validar que la fecha tenga el formato correcto
                if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
                {
                    return BadRequest(new { message = "Formato de fecha inválido. Use yyyy-MM-dd. Ejemplo: 2026-06-13" });
                }

                fechaStr = fecha;
            }

            var filePath = Path.Combine(_env.ContentRootPath, "Logs", "Auditoria", $"auditoria-{fechaStr}.txt");

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = $"No existe archivo de auditoría para la fecha {fechaStr}." });

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName  = $"auditoria-{fechaStr}.txt";

            return File(fileBytes, "text/plain; charset=utf-8", fileName);
        }

        // GET: api/auditlog/archivos
        /// <summary>
        /// Lista todos los archivos de auditoría .txt disponibles en disco.
        /// Útil para que el administrador sepa qué fechas tienen registros.
        /// </summary>
        [HttpGet("archivos")]
        public IActionResult ListarArchivosAuditoria()
        {
            var folder = Path.Combine(_env.ContentRootPath, "Logs", "Auditoria");

            if (!Directory.Exists(folder))
                return Ok(new { archivos = Array.Empty<string>() });

            var archivos = Directory
                .GetFiles(folder, "auditoria-*.txt")
                .Select(f => Path.GetFileName(f))
                .OrderByDescending(f => f)
                .ToList();

            return Ok(new { archivos });
        }
    }
}
