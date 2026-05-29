using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SuperBodega.API.Mensajeria;

public class RabbitMQProductor
{
    private readonly IConfiguration _configuration;

    public RabbitMQProductor(
        IConfiguration configuration
    )
    {
        _configuration = configuration;
    }

    public async Task Enviar(string mensaje)
    {
        bool esRailway =
    !string.IsNullOrEmpty(
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

            userName = "guest";
            password = "guest";
        }
        else
        {
            hostName =
                _configuration["RabbitMQ:LocalHost"]!;

            userName = "guest";
            password = "guest";
        }

        var factory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password
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

        var body =
            Encoding.UTF8.GetBytes(mensaje);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "ventas",
            body: body
        );

        Console.WriteLine(
            $"Mensaje enviado a RabbitMQ ({hostName})"
        );
    }
}