using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GeoPagos.Authorization.Domain.Services
{
    public class AuthorizationRequestSegundoService : IAuthorizationRequestService
    {
        private readonly IAuthorizationRequestRepository _authorizationRequestRepository;
        private readonly ILogger<AuthorizationRequestSegundoService> _logger;
        private readonly HttpClient _client;
        public AuthorizationRequestSegundoService(IAuthorizationRequestRepository authorizationRequestRepository
            , ILogger<AuthorizationRequestSegundoService> logger)
        {
            _authorizationRequestRepository = authorizationRequestRepository;
            _logger = logger;   
            _client = new HttpClient();
        }

        public async Task<AuthorizationRequestResponseDto> Authorize(AuthorizationRequestDto model)
        {
            var result = new AuthorizationRequestResponseDto();


            var verify = await VerifyAmountPayment(model);
            if (verify.Response == "Approved")
            {
                if (model.TransactionType != "Confirmation") 
                {
                    result.Message = "Payment pending";
                    var authorizationRequest = new AuthorizationRequest
                    {
                        Id = Guid.Empty,
                        TransactionId = model.TransactionId,
                        TransactionDate = model.TransactionDate,
                        Amount = model.Amount,
                        Status = "Pending",
                        CustomerName = model.CustomerName,
                        CustomerType = model.CustomerType,
                        CreatedAt = DateTime.Now,
                        TransactionType = model.TransactionType
                    };
                } else 
                {
                    result.Message = "Payment approved";
                    var authorizationRequest = new AuthorizationRequest
                    {
                        Id = Guid.Empty,
                        TransactionId = model.TransactionId,
                        TransactionDate = model.TransactionDate,
                        Amount = model.Amount,
                        Status = "Approved",
                        CustomerName = model.CustomerName,
                        CustomerType = model.CustomerType,
                        CreatedAt = DateTime.Now,
                        TransactionType = model.TransactionType
                    };

                    //Enviar a la cola de mensajes  rabbitmq
                }

                //Guardar en la base de datos
                //


            }
            else 
            {
                result.Message = "Payment rejected";    
            }

            return result;  
        }

        private async Task<PaymentDto> VerifyAmountPayment(AuthorizationRequestDto model)
        {

            // Crea el objeto JSON que deseas enviar en el cuerpo de la solicitud
            var data  = new PaymentDto()
            {
                Amount = model.Amount,
                CustomerId = model.CustomerName
            };

            try
            {
                // URL del servicio al que quieres hacer la solicitud POST
                string url = "https://api.example.com/endpoint";  // Cambia por tu URL real


                // Serializa el objeto a JSON
                string jsonContent = "";// JsonConverter Json.JsonConvert.SerializeObject(data);

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
    }
}
