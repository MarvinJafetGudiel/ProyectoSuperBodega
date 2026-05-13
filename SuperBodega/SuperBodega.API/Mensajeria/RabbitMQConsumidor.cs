using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace SuperBodega.API.Mensajeria;

public class RabbitMQConsumidor
{
    public async Task Escuchar()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
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

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var mensaje = Encoding.UTF8.GetString(body);

            Console.WriteLine("MENSAJE RECIBIDO:");
            Console.WriteLine(mensaje);
            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: "ventas",
            autoAck: true,
            consumer: consumer
        );

      
        await Task.Delay(-1);
    }
}