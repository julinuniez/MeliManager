using Microsoft.AspNetCore.Http; // Necesario para usar StatusCodes
using Microsoft.AspNetCore.Mvc;
using MeliManager.Core.Models;
using MeliManager.Data;
using MeliManager.Services;

namespace MeliManager.Core.Controllers
{
    // Creamos la clase para tipar la respuesta
    public class RespuestaWebhook
    {
        public string Mensaje { get; set; } = string.Empty;
        public long? CuentaDetectada { get; set; }
        public string? RecursoA_Consultar { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly VentaService _ventasService;
        private readonly AppDbContext _context; // Agregamos el acceso a la base de datos

        // Inyectamos ambos servicios
        public WebhookController(VentaService ventasService, AppDbContext context)
        {
            _ventasService = ventasService;
            _context = context;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RespuestaWebhook), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecibirNotificacion([FromBody] NotificacionMeli notificacion)
        {
            if (notificacion.Topic != "orders_v2")
            {
                return Ok(new RespuestaWebhook { Mensaje = "Ignorada. No es orden." });
            }

            long idCuentaVendedora = notificacion.UserId;
            string rutaOrden = notificacion.Resource;

            try
            {
                // 1. Vamos a buscar la orden real a ML
                var detallesOrden = await _ventasService.ObtenerDetallesOrden(idCuentaVendedora, rutaOrden);

                // 2. Extraemos a quién le vendimos y qué le vendimos
                long idComprador = detallesOrden.Buyer.Id;
                string skuVendido = detallesOrden.OrderItems.FirstOrDefault()?.Item?.SellerSku ?? string.Empty;

                // 3. Buscamos en tu catálogo qué mensaje le corresponde a este producto
                var producto = await _context.Productos.FindAsync(skuVendido);

                // Si le pusiste un mensaje personalizado en la base de datos usa ese, sino uno genérico
                string textoMensaje = !string.IsNullOrEmpty(producto?.MensajePostVenta)
                                      ? producto.MensajePostVenta
                                      : "¡Hola! Muchas gracias por tu compra.";

                // Le pasamos la ruta que guardaste en SQLite (ej: "C:\\Manuales\\alarma-moto.pdf")
                string? rutaManual = producto?.RutaManualPdf;

                await _ventasService.EnviarMensajePostVenta(idCuentaVendedora, idComprador, detallesOrden.Id, textoMensaje, rutaManual);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando webhook: {ex.Message}");
            }

            return Ok(new RespuestaWebhook
            {
                Mensaje = "Aviso recibido, orden procesada y mensaje enviado",
                CuentaDetectada = idCuentaVendedora,
                RecursoA_Consultar = rutaOrden
            });
        }

        [HttpPost("crear-usuario-prueba")]
        public async Task<IActionResult> CrearUsuarioPrueba(string accessToken)
        {
            using var client = new HttpClient();

            // Le decimos a Mercado Libre quiénes somos pasándole tu llave maestra
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Le indicamos que queremos el usuario para Argentina (MLA)
            var datosParaEnviar = new StringContent(
                "{\"site_id\":\"MLA\"}",
                System.Text.Encoding.UTF8,
                "application/json"
            );

            // Disparamos la orden de creación
            var respuesta = await client.PostAsync("https://api.mercadolibre.com/users/test_user", datosParaEnviar);

            // Leemos la cuenta recién creada que nos devuelve
            var contenidoJson = await respuesta.Content.ReadAsStringAsync();

            return Ok(contenidoJson);
        }

        [HttpGet("renovar-token")]
        public async Task<IActionResult> RenovarToken()
        {
            // Tu último refresh token activo actualizado
            string miRefreshToken = "TG-6a22094efbd9ea0001034c03-1005658071";

            try
            {
                var cuentaActualizada = await _ventasService.RefrescarTokenYGuardar(miRefreshToken);

                // Ahora devolvemos la entidad de la base de datos para verificar que se guardó bien
                return Ok(new
                {
                    Mensaje = "Tokens actualizados y guardados en SQLite con éxito",
                    CuentaId = cuentaActualizada.Id,
                    NuevoRefreshToken = cuentaActualizada.RefreshToken,
                    VenceEl = cuentaActualizada.FechaExpiracionToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}