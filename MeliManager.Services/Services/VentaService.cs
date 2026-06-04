using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MeliManager.Data;
using MeliManager.Core.Models;

namespace MeliManager.Services
{
    // Corregido el nombre a VentaService (sin la S)
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

        // Este es el nuevo motor que reemplaza al anterior
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

            // Lógica para guardar en SQLite
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
    }
}