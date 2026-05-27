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

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: api/auditlog
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogService.GetAllLogsAsync();
            return Ok(logs);
        }

        // GET: api/auditlog/byevent?eventType=DuplicateReference
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
    }
}