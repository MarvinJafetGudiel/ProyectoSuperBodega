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


var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);


builder.Services.AddSingleton<RabbitMQProductor>();

builder.Services.AddSingleton<ServicioCorreo>();


var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();
}



app.UseSwagger();

app.UseSwaggerUI();



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



var consumidor = new RabbitMQConsumidor(
    builder.Configuration
);

Task.Run(async () =>
{
    try
    {
        await consumidor.Escuchar();
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Error RabbitMQ: {ex.Message}"
        );
    }
});



app.Run();