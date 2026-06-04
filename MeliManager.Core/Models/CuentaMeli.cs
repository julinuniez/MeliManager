using System;

namespace MeliManager.Core.Models
{
    public class CuentaMeli
    {
        // Usamos el ID real de Mercado Libre (User ID) como Llave Primaria.
        // Como vimos en tu respuesta JSON (ej. 1005658071), ML usa números largos.
        public long Id { get; set; }

        // Un nombre para identificar la cuenta en tu sistema (ej: "Cuenta Alarms Principal")
        public string Nickname { get; set; } = string.Empty;

        // Las llaves secretas de acceso de esta cuenta específica
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        // Truco de ingeniería: Guardar el momento exacto en que vence el token.
        // Así, antes de hacer cualquier consulta, el sistema verifica si pasaron las 6 horas
        // y decide si tiene que usar el Refresh Token de forma automática.
        public DateTime FechaExpiracionToken { get; set; }

        // Fecha de registro en el sistema
        public DateTime FechaVinculacion { get; set; } = DateTime.UtcNow;
    }
}