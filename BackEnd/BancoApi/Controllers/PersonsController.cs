using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BancoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly IPersonService _personService;


        public PersonsController(IPersonService personService)
        {
            _personService = personService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _personService.GetAllAsync();
            return Ok(list);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var p = await _personService.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PersonDTO dto)
        {
            var (success, message, data) = await _personService.CreateAsync(dto);
            if (!success) return BadRequest(new { message });
            return CreatedAtAction(nameof(Get), new { id = data.Id }, data);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PersonDTO dto)
        {
            var (success, message) = await _personService.UpdateAsync(id, dto);
            if (!success) return BadRequest(new { message });
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (success, message) = await _personService.DeleteAsync(id);
            if (!success) return BadRequest(new { message });
            return NoContent();
        }
    }
}