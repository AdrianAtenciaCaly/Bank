using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BancoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementsController : ControllerBase
    {
        private readonly IMovementService _movementService;

        public MovementsController(IMovementService service) => _movementService = service;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MovementDTO dto)
        {
            var (success, message, data) = await _movementService.CreateAsync(dto);
            if (!success) return BadRequest(new { message });
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movimientos = await _movementService.GetAllAsync();
            return Ok(movimientos);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetByAccountAndDate([FromQuery] Guid accountId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var movimientos = await _movementService.GetByFilterAsync(accountId, start, end);
            return Ok(movimientos);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _movementService.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Movimiento no encontrado" });
            return NoContent();
        }
    }
}
