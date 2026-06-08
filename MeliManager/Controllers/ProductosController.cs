using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MeliManager.Core.Models;
using MeliManager.Core.DTOs;
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

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            // Va a la base de datos real y devuelve todo tu inventario
            return await _context.Productos.ToListAsync();
        }

        // POST: api/productos
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

        // POST: api/productos/calcular-costo-importacion
        [HttpPost("calcular-costo-importacion")]
        public async Task<IActionResult> CalcularYGuardarCosto(string sku, [FromBody] SimulacionImportacion datosImportacion)
        {
            try
            {
                using var client = new HttpClient();
                var respuestaDolar = await client.GetAsync("https://dolarapi.com/v1/dolares/oficial");
                var contenidoDolar = await respuestaDolar.Content.ReadAsStringAsync();

                var cotizacionOficial = JsonSerializer.Deserialize<DolarResponse>(contenidoDolar);

                if (cotizacionOficial == null || cotizacionOficial.Venta <= 0)
                {
                    return BadRequest("No se pudo obtener la cotización del dólar en este momento.");
                }

                datosImportacion.TipoCambio = cotizacionOficial.Venta;
                decimal costoUnitarioFinal = Math.Round(datosImportacion.CostoUnitarioArs, 2);

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

        // PUT: api/productos/{sku}/configurar-mensajeria
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

        // NUEVO MOTOR DE SINCRONIZACIÓN DIRECTA (SIN TOKENS)
        [HttpPost("sincronizar-mercadolibre")]
        public async Task<IActionResult> SincronizarDesdeMercadoLibre()
        {
            Console.WriteLine(">>> 1. INICIANDO SINCRONIZACION DIRECTA");

            // TUS IDS REALES DE MERCADO LIBRE CON EL PREFIJO MLA
            var misProductosReales = new List<string>
            {
                "MLA1782914275",
                "MLA3305622150",
                "MLA1782914271"
            };

            try
            {
                using var client = new HttpClient();

                // No mandamos token, pasamos como un navegador de usuario normal para evitar bloqueos
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                Console.WriteLine(">>> 2. Consultando tus productos reales en ML...");
                var idsString = string.Join(",", misProductosReales);

                // El endpoint /items es público, no requiere Token y nos da todo el detalle
                var itemsResponse = await client.GetAsync($"https://api.mercadolibre.com/items?ids={idsString}");

                if (!itemsResponse.IsSuccessStatusCode)
                {
                    return StatusCode(500, new { Error = "Error al conectar con Mercado Libre." });
                }

                var itemsData = JsonSerializer.Deserialize<List<MlItemResponse>>(
                    await itemsResponse.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                int actualizados = 0;
                int nuevos = 0;

                foreach (var itemResult in itemsData!)
                {
                    // Evitamos procesar si ML devolvió error en alguno de los IDs
                    if (itemResult.Code != 200 || itemResult.Body == null) continue;

                    var mlItem = itemResult.Body;
                    var productoLocal = await _context.Productos.FindAsync(mlItem.Id);

                    if (productoLocal != null)
                    {
                        // Modo Espejo: Actualiza tus precios y stock en tiempo real
                        productoLocal.Nombre = mlItem.Title;
                        productoLocal.PrecioVenta = mlItem.Price;
                        productoLocal.Stock = mlItem.Available_quantity;
                        productoLocal.Estado = mlItem.Status == "active" ? "publicado" : "pausado";
                        productoLocal.ImagenUrl = mlItem.Secure_thumbnail;
                        actualizados++;
                    }
                    else
                    {
                        // Guarda tus productos reales nuevos en SQLite local
                        _context.Productos.Add(new Producto
                        {
                            Sku = mlItem.Id,
                            Nombre = mlItem.Title,
                            PrecioVenta = mlItem.Price,
                            Stock = mlItem.Available_quantity,
                            Estado = mlItem.Status == "active" ? "publicado" : "pausado",
                            ImagenUrl = mlItem.Secure_thumbnail,
                            FechaAlta = DateTime.UtcNow
                        });
                        nuevos++;
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($">>> 3. BASE DE DATOS GUARDADA CON ÉXITO: {nuevos} Nuevos, {actualizados} Actualizados.");

                return Ok(new
                {
                    Mensaje = "Sincronización de productos reales Exitosa",
                    NuevosRegistrados = nuevos,
                    PublicacionesActualizadas = actualizados
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> ERROR CRITICO: " + ex.ToString());
                return StatusCode(500, new { Error = "Error crítico interno", Detalle = ex.Message });
            }
        }
    }
}