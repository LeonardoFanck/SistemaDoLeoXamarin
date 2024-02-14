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
    public class TelasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TelasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Telas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tela>>> GetTelas()
        {
          if (_context.Telas == null)
          {
              return NotFound();
          }
            return await _context.Telas.ToListAsync();
        }

        // GET: api/Telas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tela>> GetTela(int id)
        {
          if (_context.Telas == null)
          {
              return NotFound();
          }
            var tela = await _context.Telas.FindAsync(id);

            if (tela == null)
            {
                return NotFound();
            }

            return tela;
        }

        // PUT: api/Telas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTela(int id, Tela tela)
        {
            if (id != tela.Id)
            {
                return BadRequest();
            }

            _context.Entry(tela).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TelaExists(id))
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

        // POST: api/Telas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tela>> PostTela(Tela tela)
        {
          if (_context.Telas == null)
          {
              return Problem("Entity set 'AppDbContext.Telas'  is null.");
          }
            _context.Telas.Add(tela);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTela", new { id = tela.Id }, tela);
        }

        // DELETE: api/Telas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTela(int id)
        {
            if (_context.Telas == null)
            {
                return NotFound();
            }
            var tela = await _context.Telas.FindAsync(id);
            if (tela == null)
            {
                return NotFound();
            }

            _context.Telas.Remove(tela);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TelaExists(int id)
        {
            return (_context.Telas?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
