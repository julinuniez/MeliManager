var builder = WebApplication.CreateBuilder(args);

// Agregar servicios para OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Activar Swagger para probar la API visualmente
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint de prueba
app.MapGet("/", () => "Sistema de Gestión ML Activo y Escuchando");

app.Run();