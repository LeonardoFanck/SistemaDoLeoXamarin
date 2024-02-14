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
    public class ProximoRegistroController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProximoRegistroController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ProximoRegistroes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProximoRegistro>>> GetProximoRegistros()
        {
          if (_context.ProximoRegistros == null)
          {
              return NotFound();
          }
            return await _context.ProximoRegistros.ToListAsync();
        }

        // GET: api/ProximoRegistroes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProximoRegistro>> GetProximoRegistro(int id)
        {
          if (_context.ProximoRegistros == null)
          {
              return NotFound();
          }
            var proximoRegistro = await _context.ProximoRegistros.FindAsync(id);

            if (proximoRegistro == null)
            {
                return NotFound();
            }

            return proximoRegistro;
        }

        // PUT: api/ProximoRegistroes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProximoRegistro(int id, ProximoRegistro proximoRegistro)
        {
            if (id != proximoRegistro.Id)
            {
                return BadRequest();
            }

            _context.Entry(proximoRegistro).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProximoRegistroExists(id))
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

        // POST: api/ProximoRegistroes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProximoRegistro>> PostProximoRegistro(ProximoRegistro proximoRegistro)
        {
          if (_context.ProximoRegistros == null)
          {
              return Problem("Entity set 'AppDbContext.ProximoRegistros'  is null.");
          }
            _context.ProximoRegistros.Add(proximoRegistro);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProximoRegistro", new { id = proximoRegistro.Id }, proximoRegistro);
        }

        // DELETE: api/ProximoRegistroes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProximoRegistro(int id)
        {
            if (_context.ProximoRegistros == null)
            {
                return NotFound();
            }
            var proximoRegistro = await _context.ProximoRegistros.FindAsync(id);
            if (proximoRegistro == null)
            {
                return NotFound();
            }

            _context.ProximoRegistros.Remove(proximoRegistro);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProximoRegistroExists(int id)
        {
            return (_context.ProximoRegistros?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
