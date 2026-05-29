using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SuperBodega.API.Mensajeria;

public class RabbitMQConsumidor
{
    private readonly IConfiguration? _configuration;

    public RabbitMQConsumidor()
    {
    }

    public RabbitMQConsumidor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Escuchar()
    {
        bool esDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        
        var hostName = esDocker ? "rabbitmq" : (_configuration?["RabbitMQ:HostName"] ?? "localhost");
        var userName = _configuration?["RabbitMQ:UserName"] ?? "guest";
        var password = _configuration?["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "ventas",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var mensaje = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Mensaje recibido: {mensaje}");

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: "ventas",
            autoAck: true,
            consumer: consumer
        );

        Console.WriteLine($"Consumidor RabbitMQ iniciado en el host: {hostName}");

        await Task.Delay(-1);
    }
}