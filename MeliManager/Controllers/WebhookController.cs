using Microsoft.AspNetCore.Mvc;
using MeliManager.Core.Models;
using MeliManager.Data;
using MeliManager.Services;

namespace MeliManager.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly VentasService _ventasService;

        // Inyectamos el servicio en lugar del DbContext
        public WebhookController(VentasService ventasService)
        {
            _ventasService = ventasService;
        }

        [HttpPost]
        public async Task<IActionResult> RecibirNotificacion([FromBody] Venta datosVenta)
        {
            // El controlador solo delega el trabajo al servicio
            var ventaProcesada = await _ventasService.ProcesarNuevaVenta(datosVenta);

            return Ok(new
            {
                Mensaje = "Venta procesada con éxito",
                Venta = ventaProcesada
            });
        }
    }
}