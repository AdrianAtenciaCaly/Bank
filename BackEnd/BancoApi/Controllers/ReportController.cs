using Banco.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BancoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reporteService;

        public ReportController(IReportService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("movimientos")]
        public async Task<IActionResult> GetReporteMovimientos(
         [FromQuery] Guid clienteId,
         [FromQuery] DateTime fechaInicio,
         [FromQuery] DateTime fechaFin)
        {
            var result = await _reporteService.GetReportAsync(clienteId, fechaInicio, fechaFin);
            return Ok(result);
        }
    }
}