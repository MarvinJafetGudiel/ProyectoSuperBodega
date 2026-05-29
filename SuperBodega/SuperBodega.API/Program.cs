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

#region DETECCIÓN AUTOMÁTICA DE ENTORNO

string? connectionString;

// Railway SIEMPRE tendrá DATABASE_URL
var railwayDatabaseUrl =
    Environment.GetEnvironmentVariable("DATABASE_URL");

bool esRailway =
    !string.IsNullOrEmpty(railwayDatabaseUrl);

bool esDocker =
    Environment.GetEnvironmentVariable(
        "DOTNET_RUNNING_IN_CONTAINER"
    ) == "true";

if (esRailway)
{
    Console.WriteLine("--> ENTORNO: RAILWAY");

    connectionString =
        Environment.GetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection"
        );
}
else if (esDocker)
{
    Console.WriteLine("--> ENTORNO: DOCKER");

    connectionString =
        builder.Configuration.GetConnectionString(
            "DockerInternalConnection"
        );
}
else
{
    Console.WriteLine("--> ENTORNO: LOCAL");

    connectionString =
        builder.Configuration.GetConnectionString(
            "LocalConnection"
        );
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

#endregion

builder.Services.AddSingleton<RabbitMQProductor>();
builder.Services.AddSingleton<RabbitMQConsumidor>();
builder.Services.AddSingleton<ServicioCorreo>();

var app = builder.Build();

#region MIGRACIONES AUTOMÁTICAS

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    int intentos = 0;
    bool conectado = false;

    while (intentos < 6 && !conectado)
    {
        try
        {
            intentos++;

            Console.WriteLine(
                $"--> Intentando conectar a PostgreSQL ({intentos}/6)"
            );

            db.Database.Migrate();

            Console.WriteLine(
                "--> Base de datos conectada correctamente."
            );

            conectado = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"--> Error PostgreSQL: {ex.Message}"
            );

            Thread.Sleep(5000);
        }
    }
}

#endregion

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#region RABBITMQ

var consumidor =
    app.Services.GetRequiredService<RabbitMQConsumidor>();

Task.Run(async () =>
{
    int intentos = 0;

    while (intentos < 6)
    {
        try
        {
            await consumidor.Escuchar();
            break;
        }
        catch (Exception ex)
        {
            intentos++;

            Console.WriteLine(
                $"--> RabbitMQ no listo: {ex.Message}"
            );

            await Task.Delay(5000);
        }
    }
});

#endregion

app.Run();