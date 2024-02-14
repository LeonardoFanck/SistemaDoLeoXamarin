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
    public class FormaPgtoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FormaPgtoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FormaPgtoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormaPgto>>> GetFormaPgtos()
        {
          if (_context.FormaPgtos == null)
          {
              return NotFound();
          }
            return await _context.FormaPgtos.ToListAsync();
        }

        // GET: api/FormaPgtoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FormaPgto>> GetFormaPgto(int id)
        {
          if (_context.FormaPgtos == null)
          {
              return NotFound();
          }
            var formaPgto = await _context.FormaPgtos.FindAsync(id);

            if (formaPgto == null)
            {
                return NotFound();
            }

            return formaPgto;
        }

        // PUT: api/FormaPgtoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFormaPgto(int id, FormaPgto formaPgto)
        {
            if (id != formaPgto.Id)
            {
                return BadRequest();
            }

            _context.Entry(formaPgto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FormaPgtoExists(id))
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

        // POST: api/FormaPgtoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<FormaPgto>> PostFormaPgto(FormaPgto formaPgto)
        {
          if (_context.FormaPgtos == null)
          {
              return Problem("Entity set 'AppDbContext.FormaPgtos'  is null.");
          }
            _context.FormaPgtos.Add(formaPgto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFormaPgto", new { id = formaPgto.Id }, formaPgto);
        }

        // DELETE: api/FormaPgtoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFormaPgto(int id)
        {
            if (_context.FormaPgtos == null)
            {
                return NotFound();
            }
            var formaPgto = await _context.FormaPgtos.FindAsync(id);
            if (formaPgto == null)
            {
                return NotFound();
            }

            _context.FormaPgtos.Remove(formaPgto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FormaPgtoExists(int id)
        {
            return (_context.FormaPgtos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
