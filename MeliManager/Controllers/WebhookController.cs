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

        // Inyectamos el servicio en lugar del DbContext
        public WebhookController(VentaService ventasService)
        {
            _ventasService = ventasService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RespuestaWebhook), StatusCodes.Status200OK)]
        public IActionResult RecibirNotificacion([FromBody] NotificacionMeli notificacion)
        {
            // 1. Filtramos para asegurarnos de que sea un aviso de venta y no otra cosa (como una pregunta)
            if (notificacion.Topic != "orders_v2")
            {
                return Ok(new RespuestaWebhook
                {
                    Mensaje = "Notificación ignorada. No es una orden."
                });
            }

            // 2. Extraemos los datos vitales
            long idCuentaVendedora = notificacion.UserId;
            string rutaOrden = notificacion.Resource;

            // Todo: Acá llamaremos a VentaService para que busque la cuenta idCuentaVendedora
            // en SQLite, valide el token y vaya a buscar los datos a rutaOrden.

            // ML requiere que respondamos rápido con un 200 OK para saber que recibimos el aviso
            return Ok(new RespuestaWebhook
            {
                Mensaje = "Aviso recibido correctamente",
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