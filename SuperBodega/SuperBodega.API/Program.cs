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

string? connectionString =
    Environment.GetEnvironmentVariable(
        "ConnectionStrings__DefaultConnection"
    );

bool esRailway =
    !string.IsNullOrWhiteSpace(connectionString);

bool esDocker =
    Environment.GetEnvironmentVariable(
        "DOTNET_RUNNING_IN_CONTAINER"
    ) == "true";

if (esRailway)
{
    Console.WriteLine("--> ENTORNO: RAILWAY");
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

builder.Services.AddSingleton<RabbitMQProductor>();
builder.Services.AddSingleton<RabbitMQConsumidor>();
builder.Services.AddSingleton<ServicioCorreo>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    int intentos = 0;

    while (intentos < 6)
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

            break;
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

app.UseSwagger();
app.UseSwaggerUI();

if (!esRailway)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

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

app.Run();