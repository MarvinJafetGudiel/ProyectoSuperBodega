using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

namespace SuperBodega.API.Mensajeria;

public class RabbitMQProductor
{
    public async Task Enviar(string mensaje)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq"
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