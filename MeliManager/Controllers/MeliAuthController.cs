using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeliManager.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeliAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _tokenFilePath = Path.Combine(AppContext.BaseDirectory, "melitokens.json");

        public MeliAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("intercambiar-codigo")]
        public async Task<IActionResult> IntercambiarCodigo([FromBody] CodeDto requestDto)
        {
            string code = requestDto.Code;
            Console.WriteLine($">>> 1. Recibido código temporal de Vue: {code}");

            var appId = _configuration["MercadoLibre:AppId"];
            var secretKey = _configuration["MercadoLibre:SecretKey"];
            var redirectUri = _configuration["MercadoLibre:RedirectUri"];

            try
            {
                using var client = new HttpClient();

                // Ahora sí podemos usar 'data' sin conflictos
                var data = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "client_id", appId! },
                    { "client_secret", secretKey! },
                    { "code", code },
                    { "redirect_uri", redirectUri! }
                };

                var content = new FormUrlEncodedContent(data);

                Console.WriteLine(">>> 2. Solicitando Access Token oficial a Mercado Libre...");
                var response = await client.PostAsync("https://api.mercadolibre.com/oauth/token", content);
                var jsonResult = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($">>> ERROR en intercambio: {jsonResult}");
                    return BadRequest(new { Error = "No se pudo cambiar el código por el token.", Detalle = jsonResult });
                }

                // Guardamos los tokens en el archivo local
                await System.IO.File.WriteAllTextAsync(_tokenFilePath, jsonResult);
                Console.WriteLine(">>> 3. ¡TOKEN GUARDADO CON ÉXITO en melitokens.json!");

                return Ok(new { Mensaje = "Autenticación oficial completada con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    // Pequeño DTO para recibir el código desde Vue
    public class CodeDto
    {
        public string Code { get; set; } = string.Empty;
    }
}