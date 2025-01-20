using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.Services.AuthorizationRequest
{
    public abstract class AuthorizacionRequestBase
    {
        private readonly HttpClient _client;
        private readonly IAuthorizationRequestRepository _authorizationRequestRepository;
        private readonly ConnectionFactory _factory;
        public AuthorizacionRequestBase(IAuthorizationRequestRepository authorizationRequestRepository)
        {
            _client = new HttpClient();
            _factory = new ConnectionFactory()
            {
                HostName = "services-rabbitmq",
                UserName = "user",
                Password = "password"
            };
            _authorizationRequestRepository = authorizationRequestRepository;
        }

        public virtual async Task<PaymentDto> VerifyAmountPayment(AuthorizationRequestDto model)
        {
            //var discovery = await GetServiceAddress("Payment-Processor-Service");
            // Crea el objeto JSON que deseas enviar en el cuerpo de la solicitud
            var data = new PaymentDto()
            {
                Amount = model.Amount,
                CustomerName = model.CustomerName,
                TransactionId = model.TransactionId
            };

            try
            {
                // URL del servicio al que quieres hacer la solicitud POST
                string url = $"http://services-payment-processor/api/payments/";  // Cambia por tu URL real


                string jsonContent = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true // Formato con sangría para mayor legibilidad
                });


                // Crea el contenido HTTP (cuerpo de la solicitud)
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Realiza la solicitud POST
                HttpResponseMessage response = await _client.PostAsync(url, content);

                // Si la respuesta es exitosa, lee el contenido
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    data.Response = responseContent;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    data.Error = responseContent;
                }
            }
            catch (Exception ex)
            {
                data.Error = ex.Message;
            }
            return data;
        }

        public async Task SendMessage(string queueName, string message)
        {
            using var connection = await _factory.CreateConnectionAsync();
            IChannel channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = DeliveryModes.Transient;
            await channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false, basicProperties: props, body: messageBodyBytes);


            //Exchanges: Son los puntos de entrada para los mensajes en RabbitMQ. Los mensajes se enrutan a las colas basadas en las reglas de enrutamiento.
            //Bindings: Conectan exchanges y colas, definiendo cómo se enrutan los mensajes.
            //Routing keys: Se utilizan para enrutar mensajes a colas específicas.
            //Durable queues: Las colas durables persisten incluso si el servidor se reinicia.
            //ACK: El mecanismo de confirmación(ACK) garantiza que los mensajes se procesen solo una vez.
            //Dead letter exchanges: Se utilizan para manejar mensajes que no pueden ser procesados.
            //MassTransit: Una biblioteca de alto nivel que simplifica la integración con RabbitMQ en .NET.

        }

    }
}
