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
    public class PedidoItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidoItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PedidoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoItem>>> GetPedidoItens()
        {
          if (_context.PedidoItens == null)
          {
              return NotFound();
          }
            return await _context.PedidoItens.ToListAsync();
        }

        // GET: api/PedidoItems
        [HttpGet("Pedido/{id}")]
        public async Task<ActionResult<IEnumerable<PedidoItem>>> GetPedidoItensByPedido(int id)
        {
            if (_context.PedidoItens == null)
            {
                return NotFound();
            }

            return await _context.PedidoItens.Where(p => p.PedidoId == id).ToListAsync();
        }

        // GET: api/PedidoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoItem>> GetPedidoItem(int id)
        {
          if (_context.PedidoItens == null)
          {
              return NotFound();
          }
            var pedidoItem = await _context.PedidoItens.FindAsync(id);

            if (pedidoItem == null)
            {
                return NotFound();
            }

            return pedidoItem;
        }

        // PUT: api/PedidoItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoItem(int id, PedidoItem pedidoItem)
        {
            if (id != pedidoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(pedidoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                await _context.Database.ExecuteSqlRawAsync("atualizaEstoque {0}", pedidoItem.ProdutoId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoItemExists(id))
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

        // POST: api/PedidoItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PedidoItem>> PostPedidoItem(PedidoItem pedidoItem)
        {
          if (_context.PedidoItens == null)
          {
              return Problem("Entity set 'AppDbContext.PedidoItens'  is null.");
          }
            _context.PedidoItens.Add(pedidoItem);

            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlRawAsync("atualizaEstoque {0}", pedidoItem.ProdutoId);

            return CreatedAtAction("GetPedidoItem", new { id = pedidoItem.Id }, pedidoItem);
        }

        // DELETE: api/PedidoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoItem(int id)
        {
            if (_context.PedidoItens == null)
            {
                return NotFound();
            }
            var pedidoItem = await _context.PedidoItens.FindAsync(id);
            if (pedidoItem == null)
            {
                return NotFound();
            }

            _context.PedidoItens.Remove(pedidoItem);

            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlRawAsync("atualizaEstoque {0}", pedidoItem.ProdutoId);

            return NoContent();
        }

        private bool PedidoItemExists(int id)
        {
            return (_context.PedidoItens?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
