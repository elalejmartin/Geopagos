
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace GeoPagos.Authorization.Api.IntegrationEvents
{
    public class AuthorizationRequestConsumer : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        public AuthorizationRequestConsumer() 
        {
            _factory = new ConnectionFactory()
            {
                HostName = "services-rabbitmq",
                UserName = "user",
                Password = "password"
            };
        }   
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var connection = await _factory.CreateConnectionAsync();
            IChannel channel = await connection.CreateChannelAsync();

            string queueName = "authorization-request";
            await channel.QueueDeclareAsync(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Crear el consumidor
            var consumer = new AsyncEventingBasicConsumer(channel);

            // Suscribirse al evento cuando se recibe un mensaje
            consumer.ReceivedAsync += async (sender, e) => 
            {
                var body = e.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

           
                await channel.BasicAckAsync(e.DeliveryTag, false);
            };

            // Iniciar el consumidor para escuchar en la cola
            await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

            // Esperar hasta que el token de cancelación se active para detener el servicio
            await Task.Delay(Timeout.Infinite, stoppingToken); // Espera infinita hasta que el servicio sea detenido
        }

    }
}
