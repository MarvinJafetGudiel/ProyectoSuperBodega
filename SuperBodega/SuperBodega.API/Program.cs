using Microsoft.EntityFrameworkCore;
using SuperBodega.Infrastructure.Datos;
using System.Text.Json.Serialization;
using SuperBodega.API.Mensajeria;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<RabbitMQProductor>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


var consumidor = new RabbitMQConsumidor();

_ = Task.Run(async () =>
{
    try
    {
        await consumidor.Escuchar();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error en Consumidor: {ex.Message}");
    }
});


app.Run();