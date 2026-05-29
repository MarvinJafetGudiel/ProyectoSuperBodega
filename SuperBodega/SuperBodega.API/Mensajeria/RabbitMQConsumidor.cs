using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SuperBodega.API.Mensajeria;

public class RabbitMQConsumidor
{
    private readonly IConfiguration _configuration;

    public RabbitMQConsumidor(
        IConfiguration configuration
    )
    {
        _configuration = configuration;
    }

    public async Task Escuchar()
    {
        bool esRailway =
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(
                    "RabbitMQ__HostName"
                )
            );

        bool esDocker =
            Environment.GetEnvironmentVariable(
                "DOTNET_RUNNING_IN_CONTAINER"
            ) == "true";

        string hostName;
        string userName;
        string password;

        if (esRailway)
        {
            hostName =
                Environment.GetEnvironmentVariable(
                    "RabbitMQ__HostName"
                )!;

            userName =
                Environment.GetEnvironmentVariable(
                    "RabbitMQ__UserName"
                )!;

            password =
                Environment.GetEnvironmentVariable(
                    "RabbitMQ__Password"
                )!;
        }
        else if (esDocker)
        {
            hostName =
                _configuration["RabbitMQ:DockerHost"]!;

            userName =
                _configuration["RabbitMQ:UserName"]!;

            password =
                _configuration["RabbitMQ:Password"]!;
        }
        else
        {
            hostName =
                _configuration["RabbitMQ:LocalHost"]!;

            userName =
                _configuration["RabbitMQ:UserName"]!;

            password =
                _configuration["RabbitMQ:Password"]!;
        }

        var factory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        var connection =
            await factory.CreateConnectionAsync();

        var channel =
            await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "ventas_durable",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer =
            new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();

            var mensaje =
                Encoding.UTF8.GetString(body);

            Console.WriteLine(
                $"Mensaje recibido: {mensaje}"
            );

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: "ventas_durable",
            autoAck: true,
            consumer: consumer
        );

        Console.WriteLine(
            $"RabbitMQ conectado en: {hostName}"
        );

        await Task.Delay(-1);
    }
}