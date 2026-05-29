using Microsoft.EntityFrameworkCore;
using SuperBodega.Infrastructure.Datos;
using System.Text.Json.Serialization;
using SuperBodega.API.Mensajeria;
using SuperBodega.API.Servicios;

AppContext.SetSwitch(
    "Npgsql.EnableLegacyTimestampBehavior",
    true
);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region DETECCIÓN AUTOMÁTICA DE ENTORNOS (DOCKER / RAILWAY / LOCAL)

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    bool esDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    if (esDocker)
    {
        connectionString = builder.Configuration.GetConnectionString("DockerInternalConnection");
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString("LocalConnection");
        
    }

   
    connectionString ??= "Host=localhost;Port=5432;Database=superbodega_db;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

#endregion

builder.Services.AddSingleton<RabbitMQProductor>();
builder.Services.AddSingleton<RabbitMQConsumidor>();
builder.Services.AddSingleton<ServicioCorreo>();

var app = builder.Build();

#region MIGRACIONES AUTOMÁTICAS CON CONTROL DE RESILIENCIA (ESPERA A POSTGRES)

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    int intentosPostgres = 0;
    bool migradoConExito = false;

    while (intentosPostgres < 6 && !migradoConExito)
    {
        try
        {
            intentosPostgres++;
            db.Database.Migrate();
            Console.WriteLine("--> [ÉXITO] Base de datos conectada y migraciones aplicadas.");
            migradoConExito = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> [ESPERA] Postgres no está listo aún (Intento {intentosPostgres}/6). Reintentando en 4 segundos...");
            if (intentosPostgres >= 6)
            {
                Console.WriteLine($"CRÍTICO: No se pudo conectar a la BD después de varios intentos: {ex.Message}");
            }
            else
            {
                Thread.Sleep(4000); 
            }
        }
    }
}

#endregion

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

#region INICIALIZACIÓN ASÍNCRONA DEL CONSUMIDOR RABBITMQ

var consumidor = app.Services.GetRequiredService<RabbitMQConsumidor>();
Task.Run(async () =>
{
    int intentosRabbit = 0;
    while (intentosRabbit < 6)
    {
        try
        {
            await consumidor.Escuchar();
            break;
        }
        catch
        {
            intentosRabbit++;
            Console.WriteLine($"--> [ESPERA] Esperando que RabbitMQ esté listo... (Intento {intentosRabbit}/6)");
            await Task.Delay(4000);
        }
    }
});

#endregion

app.Run();