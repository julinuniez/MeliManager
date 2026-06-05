using MeliManager.Core.Models;
using MeliManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic; // Necesario para las listas (Attachments)
using System.IO; // Necesario para leer el PDF del disco
using System.Net.Http;
using System.Net.Http.Headers; // Necesario para avisar que es un archivo PDF
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeliManager.Services
{
    public class VentaService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public VentaService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<Venta> ProcesarNuevaVenta(Venta datosVenta)
        {
            string appId = _config["MercadoLibre:AppId"]!;
            string clientSecret = _config["MercadoLibre:ClientSecret"]!;

            var productoEnBase = await _context.Productos
                .FirstOrDefaultAsync(p => p.Sku == datosVenta.SkuProducto);

            datosVenta.CostoUnitario = productoEnBase != null ? productoEnBase.CostoUnitario : 0;

            _context.Ventas.Add(datosVenta);
            await _context.SaveChangesAsync();

            return datosVenta;
        }

        public async Task<CuentaMeli> RefrescarTokenYGuardar(string refreshTokenActual)
        {
            string appId = _config["MercadoLibre:AppId"]!;
            string clientSecret = _config["MercadoLibre:ClientSecret"]!;

            using var client = new HttpClient();

            var datosParaEnviar = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", appId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshTokenActual)
            });

            var respuesta = await client.PostAsync("https://api.mercadolibre.com/oauth/token", datosParaEnviar);
            var contenidoJson = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new Exception($"Error al renovar token: {contenidoJson}");
            }

            var datosToken = JsonSerializer.Deserialize<MeliTokenResponse>(contenidoJson);

            if (datosToken == null)
            {
                throw new Exception("No se pudo leer la respuesta.");
            }

            var cuentaExistente = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == datosToken.UserId);

            if (cuentaExistente != null)
            {
                cuentaExistente.AccessToken = datosToken.AccessToken;
                cuentaExistente.RefreshToken = datosToken.RefreshToken;
                cuentaExistente.FechaExpiracionToken = DateTime.UtcNow.AddSeconds(datosToken.ExpiresIn);
            }
            else
            {
                cuentaExistente = new CuentaMeli
                {
                    Id = datosToken.UserId,
                    Nickname = $"Usuario_{datosToken.UserId}",
                    AccessToken = datosToken.AccessToken,
                    RefreshToken = datosToken.RefreshToken,
                    FechaExpiracionToken = DateTime.UtcNow.AddSeconds(datosToken.ExpiresIn),
                    FechaVinculacion = DateTime.UtcNow
                };

                _context.Cuentas.Add(cuentaExistente);
            }

            await _context.SaveChangesAsync();

            return cuentaExistente;
        }

        public async Task<OrdenMeli> ObtenerDetallesOrden(long cuentaId, string rutaOrden)
        {
            var cuenta = await _context.Cuentas.FindAsync(cuentaId);

            if (cuenta == null)
            {
                throw new Exception($"La cuenta {cuentaId} no está registrada en el sistema.");
            }

            if (DateTime.UtcNow >= cuenta.FechaExpiracionToken)
            {
                cuenta = await RefrescarTokenYGuardar(cuenta.RefreshToken);
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {cuenta.AccessToken}");
            var urlCompleta = $"https://api.mercadolibre.com{rutaOrden}";

            var respuesta = await client.GetAsync(urlCompleta);
            var contenidoJson = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new Exception($"Error al descargar la orden de ML: {contenidoJson}");
            }

            var orden = JsonSerializer.Deserialize<OrdenMeli>(contenidoJson);

            if (orden == null)
            {
                throw new Exception("No se pudo leer la respuesta financiera de la orden.");
            }

            return orden;
        }

        // --- NUEVO MÉTODO PRIVADO: Sube el PDF a los servidores de ML ---
        private async Task<string?> SubirAdjuntoMeli(string rutaArchivo, string accessToken)
        {
            if (!File.Exists(rutaArchivo))
            {
                Console.WriteLine($"Atención: No se encontró el manual en la ruta {rutaArchivo}");
                return null;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(rutaArchivo));

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "file", Path.GetFileName(rutaArchivo));

            // Subimos al servidor de adjuntos de Argentina (MLA)
            var respuesta = await client.PostAsync("https://api.mercadolibre.com/messages/attachments?site_id=MLA", form);
            var jsonRespuesta = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error subiendo adjunto: {jsonRespuesta}");
                return null;
            }

            using var doc = JsonDocument.Parse(jsonRespuesta);
            return doc.RootElement.GetProperty("id").GetString();
        }

        // --- MÉTODO ACTUALIZADO: Ahora acepta rutaPdf y usa el subidor ---
        public async Task EnviarMensajePostVenta(long cuentaVendedoraId, long cuentaCompradoraId, long ordenId, string textoMensaje, string? rutaPdf = null)
        {
            var cuenta = await _context.Cuentas.FindAsync(cuentaVendedoraId);
            if (cuenta == null) throw new Exception("Cuenta no encontrada");

            if (DateTime.UtcNow >= cuenta.FechaExpiracionToken)
            {
                cuenta = await RefrescarTokenYGuardar(cuenta.RefreshToken);
            }

            var mensaje = new MensajeMeliRequest
            {
                From = new UsuarioMensaje { UserId = cuentaVendedoraId },
                To = new UsuarioMensaje { UserId = cuentaCompradoraId },
                Text = textoMensaje
            };

            // INYECCIÓN DEL ADJUNTO
            if (!string.IsNullOrEmpty(rutaPdf))
            {
                string? idAdjunto = await SubirAdjuntoMeli(rutaPdf, cuenta.AccessToken);

                if (!string.IsNullOrEmpty(idAdjunto))
                {
                    mensaje.Attachments = new List<string> { idAdjunto };
                }
            }

            var jsonMensaje = JsonSerializer.Serialize(mensaje);
            var contenido = new StringContent(jsonMensaje, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {cuenta.AccessToken}");

            string url = $"https://api.mercadolibre.com/messages/packs/{ordenId}/sellers/{cuentaVendedoraId}";

            var respuesta = await client.PostAsync(url, contenido);
            var respuestaJson = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new Exception($"Error al enviar mensaje: {respuestaJson}");
            }
        }
    }
}