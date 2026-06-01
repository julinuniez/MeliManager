using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Agregamos esto para leer el appsettings
using MeliManager.Data;
using MeliManager.Core.Models;

namespace MeliManager.Services
{
    public class VentasService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        // Inyectamos IConfiguration en el constructor
        public VentasService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<Venta> ProcesarNuevaVenta(Venta datosVenta)
        {
            // Así es como el servicio lee tus llaves secretas:
            string appId = _config["MercadoLibre:AppId"]!;
            string clientSecret = _config["MercadoLibre:ClientSecret"]!;

            // Todo: Acá usaremos las llaves para ir a buscar los datos reales a ML

            var productoEnBase = await _context.Productos
                .FirstOrDefaultAsync(p => p.Sku == datosVenta.SkuProducto);

            datosVenta.CostoUnitario = productoEnBase != null ? productoEnBase.CostoUnitario : 0;

            _context.Ventas.Add(datosVenta);
            await _context.SaveChangesAsync();

            return datosVenta;
        }
    }
}