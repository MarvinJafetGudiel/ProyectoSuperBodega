using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SuperBodega.API.Mensajeria;

public class RabbitMQProductor
{
    private readonly IConfiguration _configuration;

    public RabbitMQProductor(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Enviar(string mensaje)
    {
        var hostName =
            _configuration["RabbitMQ:HostName"]
            ?? "localhost";

        var factory = new ConnectionFactory()
        {
            HostName = hostName
        };

        using var connection =
            await factory.CreateConnectionAsync();

        using var channel =
            await connection.CreateChannelAsync();

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

        Console.WriteLine(
            "Mensaje enviado a RabbitMQ"
        );
    }
}