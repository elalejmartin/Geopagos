
using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace GeoPagos.Authorization.Api.IntegrationEvents
{
    public class AuthorizationRequestConsumer : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly ILogger<AuthorizationRequestConsumer> _logger;
        //private readonly IAuthorizationRequestApprovedService _authorizationRequestApprovedService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceProvider _services;
        public AuthorizationRequestConsumer(ILogger<AuthorizationRequestConsumer> logger, 
            IAuthorizationRequestApprovedService authorizationRequestApprovedService,
            IServiceScopeFactory serviceScopeFactory,
            IServiceProvider services) 
        {
            _logger = logger;
            //_authorizationRequestApprovedService = authorizationRequestApprovedService;
            _serviceScopeFactory = serviceScopeFactory;
            _services = services;
            _factory = new ConnectionFactory()
            {
                HostName = "services-rabbitmq",
                UserName = "user",
                Password = "password"
            };
        }   
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Started Consumer");
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
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation($"Received message: {message}");
                        if (!string.IsNullOrEmpty(message))
                            await MessageHandler(message);

                        await channel.BasicAckAsync(e.DeliveryTag, true);
                    };

                    // Iniciar el consumidor para escuchar en la cola
                    await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

                    _logger.LogInformation($"End Consumer");

                   

                }
                catch (Exception ex)
                {

                    // Manejar errores para evitar que el servicio se detenga
                    _logger.LogError($"Error  MessageHandler: {ex.Message}");

                }

                // Esperar 1 minuto antes de volver a ejecutar
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }

        }

        public async Task MessageHandler(string message) 
        {
            try
            {
                _logger.LogInformation($"Inicia MessageHandler: {message}");
                var item = JsonSerializer.Deserialize<AuthorizationRequestApprovedDto>(message);
                var list = new List<AuthorizationRequestApprovedDto>();
                list.Add(item);
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                var _authorizationRequestApprovedService = scope.ServiceProvider.GetService<IAuthorizationRequestApprovedService>();
                await _authorizationRequestApprovedService.SaveList(list);
                _logger.LogInformation($"Termina MessageHandler: {message}");
            }
            catch (Exception ex)
            {
                throw ex;
            }

          
      

        }

    }
}
