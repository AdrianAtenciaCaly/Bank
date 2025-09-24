using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BancoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _cuentaService;

        public CustomerController(ICustomerService cuentaService)
        {
            _cuentaService = cuentaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _cuentaService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _cuentaService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> GetByCliente(Guid clienteId)
        {
            var result = await _cuentaService.GetByIdAsync(clienteId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerDTO dto)
        {
            var (success, message, createdCustomer) = await _cuentaService.CreateAsync(dto);

            if (!success)
                return BadRequest(new { Message = message });

            return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CustomerDTO dto)
        {
            var (success, message) = await _cuentaService.UpdateAsync(id, dto);
            if (!success) return BadRequest(new { message });
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (success, message) = await _cuentaService.DeleteAsync(id);
            if (!success) return BadRequest(new { message });
            return NoContent();
        }

        [HttpGet("with-person")]
        public async Task<IActionResult> GetAllWithPerson()
        {
            var result = await _cuentaService.GetAllWithPersonAsync();
            return Ok(result);
        }
    }
}
