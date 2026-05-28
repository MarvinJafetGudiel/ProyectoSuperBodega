using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SuperBodega.API.Mensajeria;

public class RabbitMQProductor
{
    private readonly IConfiguration _configuration;

    public RabbitMQProductor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Enviar(string mensaje)
    {
        // Leemos las 3 variables desde la configuración de .NET o usamos valores locales por defecto
        var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
        var password = _configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        using var connection = await factory.CreateConnectionAsync();

        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "ventas",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(mensaje);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "ventas",
            body: body
        );

        Console.WriteLine("Mensaje enviado a RabbitMQ");
    }
}