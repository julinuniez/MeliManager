using MeliManager.Core.Models;
using MeliManager.Data;
using MeliManager.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar el DbContext para la Base de Datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=meli_datos.db"));

// 2. Registrar los servicios de la capa de negocio (Inyección de Dependencias)
// Aquí iremos agregando nuestros servicios más adelante

// 3. Configurar Controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<VentaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 4. Mapear los controladores automáticamente
app.MapControllers();

app.Run();