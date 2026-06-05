using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MeliManager.Core.Models;
using MeliManager.Data;

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

        public class ConfigurarMensajeriaDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public string RutaPdf { get; set; } = string.Empty;
        }
        // Tu método original para guardar productos de forma manual
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

        // El nuevo simulador automático de importaciones
        [HttpPost("calcular-costo-importacion")]
        public async Task<IActionResult> CalcularYGuardarCosto(string sku, [FromBody] SimulacionImportacion datosImportacion)
        {
            try
            {
                // 1. Vamos a buscar la cotización oficial a DolarAPI
                using var client = new HttpClient();
                var respuestaDolar = await client.GetAsync("https://dolarapi.com/v1/dolares/oficial");
                var contenidoDolar = await respuestaDolar.Content.ReadAsStringAsync();

                var cotizacionOficial = JsonSerializer.Deserialize<DolarResponse>(contenidoDolar);

                if (cotizacionOficial == null || cotizacionOficial.Venta <= 0)
                {
                    return BadRequest("No se pudo obtener la cotización del dólar en este momento.");
                }

                // 2. Le inyectamos el valor actualizado a tu simulador
                datosImportacion.TipoCambio = cotizacionOficial.Venta;

                // 3. El modelo ya hizo la matemática. Extraemos el costo final redondeado.
                decimal costoUnitarioFinal = Math.Round(datosImportacion.CostoUnitarioArs, 2);

                // 4. Guardamos o actualizamos en tu base de datos SQLite
                var producto = await _context.Productos.FindAsync(sku);

                if (producto != null)
                {
                    producto.CostoUnitario = costoUnitarioFinal;
                }
                else
                {
                    producto = new Producto
                    {
                        Sku = sku,
                        Nombre = $"Producto Nuevo ({sku})",
                        CostoUnitario = costoUnitarioFinal,
                        FechaAlta = DateTime.UtcNow
                    };
                    _context.Productos.Add(producto);
                }

                await _context.SaveChangesAsync();

                // 5. Devolvemos un reporte financiero completo
                return Ok(new
                {
                    Mensaje = "Costo calculado y guardado con éxito en la base de datos",
                    Producto = sku,
                    CotizacionUtilizada = new
                    {
                        Valor = cotizacionOficial.Venta,
                        FechaActualizacion = cotizacionOficial.FechaActualizacion
                    },
                    DesgloseFinanciero = new
                    {
                        CashFlowRequeridoUsd = Math.Round(datosImportacion.CashFlowTotalUsd, 2),
                        IvaCreditoFiscalUsd = Math.Round(datosImportacion.IvaAduaneroUsd, 2),
                        CostoTotalEnDestinoArs = Math.Round(datosImportacion.CostoTotalArs, 2),
                        CostoUnitarioFinalArs = costoUnitarioFinal
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Ocurrió un error en el cálculo", Detalle = ex.Message });
            }
        }

        [HttpPut("{sku}/configurar-mensajeria")]
        public async Task<IActionResult> ConfigurarMensajeria(string sku, [FromBody] ConfigurarMensajeriaDto configuracion)
        {
            var producto = await _context.Productos.FindAsync(sku);

            if (producto == null)
            {
                return NotFound($"El producto con SKU {sku} no existe en la base de datos.");
            }

            producto.MensajePostVenta = configuracion.Mensaje;
            producto.RutaManualPdf = configuracion.RutaPdf;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = "Mensajería actualizada con éxito",
                Sku = sku,
                RutaGuardada = producto.RutaManualPdf
            });
        }
    }
}