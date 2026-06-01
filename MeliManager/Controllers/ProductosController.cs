using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeliManager.Core.Models;
using MeliManager.Data;
using MeliManager.Services;

namespace MeliManager.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> GuardarProducto([FromBody] Producto nuevoProducto)
        {
            if (string.IsNullOrEmpty(nuevoProducto.Sku))
            {
                return BadRequest("El SKU es obligatorio.");
            }

            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();
            return Ok($"Producto {nuevoProducto.Sku} guardado correctamente.");
        }
    }
}