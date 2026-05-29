using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using SuperBodega.Infrastructure.Datos;
using System.Text.Json.Serialization;
using SuperBodega.API.Mensajeria;
using SuperBodega.API.Servicios;


var builder = WebApplication.CreateBuilder(args);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{

    connectionString = databaseUrl;
    Console.WriteLine("--> [CONFIG] Postgres configurado usando la variable de entorno de Railway.");
}
else
{

    connectionString = builder.Configuration.GetConnectionString("LocalConnection") 
                       ?? "Host=superbodega_postgres;Database=SuperBodegaDb;Username=postgres;Password=postgres";
    Console.WriteLine("--> [CONFIG] Postgres configurado usando el entorno Local.");
}

var railwayRabbitUrl = Environment.GetEnvironmentVariable("RABBITMQ_URL");
ConnectionFactory rabbitConnectionFactory;

if (!string.IsNullOrEmpty(railwayRabbitUrl))
{
    rabbitConnectionFactory = new ConnectionFactory()
    {
        Uri = new Uri(railwayRabbitUrl),
        DispatchConsumersAsync = true 
    };
    Console.WriteLine("--> [CONFIG] RabbitMQ configurado usando la URL de Railway.");
}
else
{

    rabbitConnectionFactory = new ConnectionFactory()
    {
        HostName = "superbodega_rabbitmq", 
        UserName = "guest",
        Password = "guest",
        DispatchConsumersAsync = true
    };
    Console.WriteLine("--> [CONFIG] RabbitMQ configurado para el entorno Local (superbodega_rabbitmq).");
}

builder.Services.Singleton(rabbitConnectionFactory);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    int maxRetries = 6;
    int delaySeconds = 5;

    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            Console.WriteLine($"--> [DB] Verificando conexión a la base de datos... (Intento {retry}/{maxRetries})");
            
            Console.WriteLine("--> [DB] ¡Conexión a la base de datos exitosa!");
            break; 
        }
        catch (Exception ex)
        {
            if (retry == maxRetries)
            {
                Console.WriteLine("--> [ERROR FATAL] No se pudo conectar a la base de datos tras agotar los intentos.");
                throw;
            }
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }

    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            Console.WriteLine($"--> [ESPERA] Esperando que RabbitMQ esté listo... (Intento {retry}/{maxRetries})");
            
            using var testConnection = rabbitConnectionFactory.CreateConnection();
            
            Console.WriteLine("--> [ÉXITO] ¡Conexión establecida con RabbitMQ con éxito!");
            break; 
        }
        catch (Exception ex)
        {
            if (retry == maxRetries)
            {
                Console.WriteLine($"--> [ERROR FATAL] RabbitMQ no respondió después de {maxRetries} intentos. Deteniendo contenedor.");
                throw;
            }
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProyectoSuperBodega API V1");

    c.RoutePrefix = string.Empty; 
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("--> [APLICACIÓN] Iniciando Web API ProyectoSuperBodega...");
app.Run();