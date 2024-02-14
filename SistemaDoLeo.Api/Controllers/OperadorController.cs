using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDoLeo.Modelos.Classes;
using XamarinAPI.DB;

namespace XamarinAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperadorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OperadorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Operadors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Operador>>> GetOperadores()
        {
          if (_context.Operadores == null)
          {
              return NotFound();
          }
            return await _context.Operadores.ToListAsync();
        }

        // GET: api/Operadors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Operador>> GetOperador(int id)
        {
          if (_context.Operadores == null)
          {
              return NotFound();
          }
            var operador = await _context.Operadores.FindAsync(id);

            if (operador == null)
            {
                return NotFound();
            }

            return operador;
        }

        // PUT: api/Operadors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOperador(int id, Operador operador)
        {
            if (id != operador.Id)
            {
                return BadRequest();
            }

            _context.Entry(operador).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperadorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Operadors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Operador>> PostOperador(Operador operador)
        {
          if (_context.Operadores == null)
          {
              return Problem("Entity set 'AppDbContext.Operadores'  is null.");
          }
            _context.Operadores.Add(operador);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOperador", new { id = operador.Id }, operador);
        }

        // DELETE: api/Operadors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOperador(int id)
        {
            if (_context.Operadores == null)
            {
                return NotFound();
            }
            var operador = await _context.Operadores.FindAsync(id);
            if (operador == null)
            {
                return NotFound();
            }

            _context.Operadores.Remove(operador);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OperadorExists(int id)
        {
            return (_context.Operadores?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
