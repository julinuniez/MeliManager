using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        // --- DTOs internos ---
        public class ConfigurarMensajeriaDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public string RutaPdf { get; set; } = string.Empty;
        }

        public class SincronizarRequestDto
        {
            public string SellerId { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }

        // NUEVO DTO: Para recibir los cambios del formulario de Vue
        public class ProductoActualizarDto
        {
            public string Nombre { get; set; } = string.Empty;
            public decimal PrecioVenta { get; set; }
            public int Stock { get; set; }
            public string Estado { get; set; } = string.Empty;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
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
                        Nombre = $"Producto Importado ({sku})",
                        CostoUnitario = costoUnitarioFinal,
                        FechaAlta = DateTime.UtcNow
                    };
                    _context.Productos.Add(producto);
                }

                var nuevoHistorial = new HistorialSimulacion
                {
                    SkuProducto = sku,
                    FechaSimulacion = DateTime.UtcNow,
                    CotizacionDolarUtilizada = cotizacionOficial.Venta,
                    ValorFobUsd = 0,
                    FleteUsd = 0,
                    CashFlowRequeridoUsd = Math.Round(datosImportacion.CashFlowTotalUsd, 2),
                    IvaCreditoFiscalUsd = Math.Round(datosImportacion.IvaAduaneroUsd, 2),
                    CostoTotalEnDestinoArs = Math.Round(datosImportacion.CostoTotalArs, 2),
                    CostoUnitarioFinalArs = costoUnitarioFinal
                };

                _context.HistorialSimulaciones.Add(nuevoHistorial);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Mensaje = "Costo calculado y archivado en el historial con éxito",
                    Producto = sku,
                    CotizacionUtilizada = cotizacionOficial.Venta,
                    DesgloseFinanciero = new
                    {
                        CashFlowRequeridoUsd = nuevoHistorial.CashFlowRequeridoUsd,
                        IvaCreditoFiscalUsd = nuevoHistorial.IvaCreditoFiscalUsd,
                        CostoTotalEnDestinoArs = nuevoHistorial.CostoTotalEnDestinoArs,
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

        // MOTOR DE SINCRONIZACIÓN AUTOMÁTICA (CATÁLOGO COMPLETO)
        [HttpPost("sincronizar-mercadolibre")]
        public async Task<IActionResult> SincronizarDesdeMercadoLibre()
        {
            Console.WriteLine(">>> 1. INICIANDO SINCRONIZACION AUTOMÁTICA");

            var tokenFilePath = Path.Combine(AppContext.BaseDirectory, "melitokens.json");

            if (!System.IO.File.Exists(tokenFilePath))
            {
                return BadRequest(new { Error = "No se encontró el token de Mercado Libre. Iniciá sesión primero." });
            }

            string tokenLimpio = "";
            try
            {
                var jsonTokens = await System.IO.File.ReadAllTextAsync(tokenFilePath);
                using var documentTokens = JsonDocument.Parse(jsonTokens);
                tokenLimpio = documentTokens.RootElement.GetProperty("access_token").GetString() ?? "";
            }
            catch
            {
                return BadRequest(new { Error = "Error al leer el token." });
            }

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenLimpio}");

                // 1. Buscamos tu ID de vendedor oficial
                Console.WriteLine(">>> 2. Identificando cuenta vendedora...");
                var meResponse = await client.GetAsync("https://api.mercadolibre.com/users/me");

                if (!meResponse.IsSuccessStatusCode)
                {
                    return BadRequest(new { Error = "Error al validar el token. Volvé a conectarte con ML." });
                }

                var meJson = await meResponse.Content.ReadAsStringAsync();
                using var meDoc = JsonDocument.Parse(meJson);
                long sellerId = meDoc.RootElement.GetProperty("id").GetInt64();
                string nickname = meDoc.RootElement.GetProperty("nickname").GetString() ?? "Vendedor";

                Console.WriteLine($">>> Hola {nickname} (ID: {sellerId}). Buscando tu catálogo...");

                // 2. Le pedimos a ML la lista de TODOS tus IDs reales (Activos, Pausados y Finalizados)
                Console.WriteLine(">>> Buscando catálogo completo (incluyendo pausadas)...");
                var urlBusqueda = $"https://api.mercadolibre.com/users/{sellerId}/items/search?status=active,paused,closed";
                var searchResponse = await client.GetAsync(urlBusqueda);

                var searchJson = await searchResponse.Content.ReadAsStringAsync();
                Console.WriteLine($">>> RESULTADO BRUTO DE BÚSQUEDA: {searchJson}");

                using var searchDoc = JsonDocument.Parse(searchJson);

                var misProductosReales = new List<string>();
                if (searchDoc.RootElement.TryGetProperty("results", out var resultados))
                {
                    foreach (var item in resultados.EnumerateArray())
                    {
                        misProductosReales.Add(item.GetString()!);
                    }
                }

                if (misProductosReales.Count == 0)
                {
                    Console.WriteLine(">>> No se encontraron publicaciones para esta cuenta.");
                    return Ok(new { Mensaje = "No tenés publicaciones activas o pausadas.", NuevosRegistrados = 0, PublicacionesActualizadas = 0 });
                }

                Console.WriteLine($">>> 3. Se encontraron {misProductosReales.Count} publicaciones tuyas. Descargando detalles...");

                // 3. Procesamos tus IDs uno por uno
                int actualizados = 0;
                int nuevos = 0;
                var logErrores = new List<string>();

                foreach (var skuId in misProductosReales)
                {
                    Console.WriteLine($">>> Descargando: {skuId}");
                    var response = await client.GetAsync($"https://api.mercadolibre.com/items/{skuId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        logErrores.Add($"Error en {skuId}");
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(json);
                    var root = document.RootElement;

                    string mlId = root.GetProperty("id").GetString() ?? string.Empty;
                    string title = root.GetProperty("title").GetString() ?? "Sin Título";
                    decimal price = root.GetProperty("price").GetDecimal();
                    int stock = root.GetProperty("available_quantity").GetInt32();
                    string status = root.GetProperty("status").GetString() ?? "paused";

                    // --- RECOLECTOR DE GALERÍA COMPLETA ---
                    var listaFotos = new List<string>();
                    string imagenAltaDefinicion = string.Empty;

                    if (root.TryGetProperty("pictures", out var pictures) && pictures.GetArrayLength() > 0)
                    {
                        foreach (var foto in pictures.EnumerateArray())
                        {
                            if (foto.TryGetProperty("secure_url", out var hdUrl) && hdUrl.ValueKind != JsonValueKind.Null)
                            {
                                listaFotos.Add(hdUrl.GetString() ?? string.Empty);
                            }
                        }

                        if (listaFotos.Count > 0)
                        {
                            imagenAltaDefinicion = listaFotos[0]; // La primera queda como portada
                        }
                    }

                    // Por si la galería falla, buscamos el thumbnail básico
                    if (string.IsNullOrEmpty(imagenAltaDefinicion))
                    {
                        if (root.TryGetProperty("secure_thumbnail", out var secureThumb) && secureThumb.ValueKind != JsonValueKind.Null)
                        {
                            imagenAltaDefinicion = secureThumb.GetString()?.Replace("-I.jpg", "-O.jpg") ?? string.Empty;
                            listaFotos.Add(imagenAltaDefinicion);
                        }
                    }

                    // Convertimos la lista de fotos a texto para guardarla en la base de datos
                    string galeriaGuardar = JsonSerializer.Serialize(listaFotos);

                    var productoLocal = await _context.Productos.FindAsync(mlId);

                    if (productoLocal != null)
                    {
                        productoLocal.Nombre = title;
                        productoLocal.PrecioVenta = price;
                        productoLocal.Stock = stock;
                        productoLocal.Estado = status == "active" ? "publicado" : "pausado";
                        productoLocal.ImagenUrl = imagenAltaDefinicion;
                        productoLocal.GaleriaJson = galeriaGuardar; // <-- Agregamos la galería
                        actualizados++;
                    }
                    else
                    {
                        _context.Productos.Add(new Producto
                        {
                            Sku = mlId,
                            Nombre = title,
                            PrecioVenta = price,
                            Stock = stock,
                            Estado = status == "active" ? "publicado" : "pausado",
                            ImagenUrl = imagenAltaDefinicion,
                            GaleriaJson = galeriaGuardar, // <-- Agregamos la galería
                            FechaAlta = DateTime.UtcNow
                        });
                        nuevos++;
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($">>> 4. BASE DE DATOS GUARDADA: {nuevos} Nuevos, {actualizados} Actualizados.");

                return Ok(new
                {
                    Mensaje = "Sincronización Exitosa",
                    NuevosRegistrados = nuevos,
                    PublicacionesActualizadas = actualizados
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> ERROR CRÍTICO: " + ex.ToString());
                return StatusCode(500, new { Error = "Error crítico interno", Detalle = ex.Message });
            }
        }

        // MOTOR DE SINCRONIZACIÓN INVERSA (ENVIAR CAMBIOS LOCALES HACIA MERCADO LIBRE)
        [HttpPut("{sku}/sincronizar-hacia-ml")]
        public async Task<IActionResult> SincronizarHaciaMercadoLibre(string sku, [FromBody] ProductoActualizarDto datos)
        {
            Console.WriteLine($">>> INICIANDO ACTUALIZACIÓN HACIA ML PARA EL SKU: {sku}");

            // 1. Buscamos la llave maestra guardada en el disco
            var tokenFilePath = Path.Combine(AppContext.BaseDirectory, "melitokens.json");
            if (!System.IO.File.Exists(tokenFilePath))
            {
                return BadRequest(new { Error = "No se encontró el token. Iniciá sesión primero." });
            }

            string tokenLimpio = "";
            try
            {
                var jsonTokens = await System.IO.File.ReadAllTextAsync(tokenFilePath);
                using var documentTokens = JsonDocument.Parse(jsonTokens);
                tokenLimpio = documentTokens.RootElement.GetProperty("access_token").GetString() ?? "";
            }
            catch
            {
                return BadRequest(new { Error = "Error al leer el archivo de token." });
            }

            // 2. Actualizamos primero tu Base de Datos Local
            var productoLocal = await _context.Productos.FindAsync(sku);
            if (productoLocal == null)
            {
                return NotFound(new { Error = $"El producto con SKU {sku} no existe localmente." });
            }

            productoLocal.Nombre = datos.Nombre;
            productoLocal.PrecioVenta = datos.PrecioVenta;
            productoLocal.Stock = datos.Stock;
            productoLocal.Estado = datos.Estado; // "publicado" o "pausado"

            await _context.SaveChangesAsync();
            Console.WriteLine($">>> 1. Base de datos local actualizada para {sku}");

            // 3. Enviamos la actualización en tiempo real a la API de Mercado Libre
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenLimpio}");

                // Mercado Libre maneja "active" o "paused" en su API
                string meliStatus = datos.Estado == "publicado" ? "active" : "paused";

                // Armamos el paquete exactamente como lo exige Mercado Libre
                var bodyMeli = new
                {
                    title = datos.Nombre,
                    price = datos.PrecioVenta,
                    available_quantity = datos.Stock,
                    status = meliStatus
                };

                string jsonBody = JsonSerializer.Serialize(bodyMeli);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // Hacemos un PUT directo al item de Mercado Libre
                var response = await client.PutAsync($"https://api.mercadolibre.com/items/{sku}", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($">>> ERROR EN ML: {responseBody}");
                    return BadRequest(new
                    {
                        Error = "Tu base de datos se actualizó, pero Mercado Libre rechazó el cambio.",
                        Detalle = responseBody
                    });
                }

                Console.WriteLine($">>> 2. ¡Sincronización exitosa en Mercado Libre para {sku}!");
                return Ok(new { Mensaje = "Producto actualizado localmente y en Mercado Libre con éxito." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> ERROR CRÍTICO: " + ex.ToString());
                return StatusCode(500, new { Error = "Error crítico en el servidor", Detalle = ex.Message });
            }
        }

        // GET: api/productos/{sku}/historial
        [HttpGet("{sku}/historial")]
        public async Task<ActionResult<IEnumerable<HistorialSimulacion>>> GetHistorial(string sku)
        {
            Console.WriteLine($">>> Buscando historial para el SKU: {sku}");

            var historial = await _context.HistorialSimulaciones
                .Where(h => h.SkuProducto == sku)
                .OrderByDescending(h => h.FechaSimulacion)
                .ToListAsync();

            return Ok(historial);
        }
    }
}