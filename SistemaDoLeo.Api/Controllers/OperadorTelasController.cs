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
    public class OperadorTelasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OperadorTelasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/OperadorTelas
        [HttpGet("{id}/get")]
        public async Task<ActionResult<OperadorTela>> GetOperadorTela(int id)
        {
            if (_context.OperadorTelas == null)
            {
                return NotFound();
            }

            var operadorTela = await _context.OperadorTelas.FindAsync(id);

            if (operadorTela == null)
            {
                return NotFound();
            }

            return operadorTela;
        }

        // GET: api/OperadorTelas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<OperadorTela>>> GetOperadorTelasByOperador(int id)
        {
            if (_context.OperadorTelas == null)
            {
                return NotFound();
            }
            var operadorTela = await _context.OperadorTelas.Where(o => o.OperadorId == id).ToListAsync();

            if (operadorTela == null)
            {
                return NotFound();
            }

            return operadorTela;
        }

        // PUT: api/OperadorTelas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOperadorTela(int id, OperadorTela operadorTela)
        {
            if (id != operadorTela.Id)
            {
                return BadRequest();
            }

            _context.Entry(operadorTela).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperadorTelaExists(id))
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

        // POST: api/OperadorTelas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<OperadorTela>> PostOperadorTela(OperadorTela operadorTela)
        {
            if (_context.OperadorTelas == null)
            {
                return Problem("Entity set 'AppDbContext.OperadorTelas'  is null.");
            }
            _context.OperadorTelas.Add(operadorTela);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOperadorTela", new { id = operadorTela.Id }, operadorTela);
        }

        // DELETE: api/OperadorTelas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOperadorTela(int id)
        {
            if (_context.OperadorTelas == null)
            {
                return NotFound();
            }
            var operadorTela = await _context.OperadorTelas.FindAsync(id);
            if (operadorTela == null)
            {
                return NotFound();
            }

            _context.OperadorTelas.Remove(operadorTela);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OperadorTelaExists(int id)
        {
            return (_context.OperadorTelas?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
