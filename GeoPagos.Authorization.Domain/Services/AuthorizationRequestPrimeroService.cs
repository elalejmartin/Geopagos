using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.Services
{
    public class AuthorizationRequestPrimeroService : AuthorizacionRequestBase, IAuthorizationRequestService
    {
        private readonly IAuthorizationRequestRepository _authorizationRequestRepository;
        private readonly ILogger<AuthorizationRequestPrimeroService> _logger;
        private readonly HttpClient _client;

        public AuthorizationRequestPrimeroService(IAuthorizationRequestRepository authorizationRequestRepository
            , ILogger<AuthorizationRequestPrimeroService> logger) : base(authorizationRequestRepository)
        {
            _authorizationRequestRepository = authorizationRequestRepository;
            _logger = logger;
            _client = new HttpClient();
        }

        public async Task<AuthorizationRequestResponseDto> Authorize(AuthorizationRequestDto model)
        {
            var result = new AuthorizationRequestResponseDto();


            var verify = await VerifyAmountPayment(model);

            switch (model.TransactionType)
            {
                case "Cobro":
                    result = await TransactionTypeCobro(model, verify.Response);
                    break;
                case "Devolucion":
                    result = await TransactionTypeDevolucion(model, verify.Response);
                    break;
                case "Reversa":
                    result = await TransactionTypeReversa(model, verify.Response);
                    break;
                default:
                    result.Message = $"Invalid TransactionType: {model.TransactionType} - Solo se admite Cobro,Devolucion,Reversa para customerType:1";
                    break;
            }

            return result;
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

        public  async Task<AuthorizationRequestResponseDto> TransactionTypeCobro(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();

            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequest = new AuthorizationRequest
            {
                Id = Guid.Empty,
                TransactionId = model.TransactionId,
                TransactionDate = model.TransactionDate,
                Amount = model.Amount,
                Status = statusProcessPayment,
                CustomerName = model.CustomerName,
                CustomerType = model.CustomerType,
                CreatedAt = DateTime.Now,
                TransactionType = model.TransactionType
            };

            //Guardar auth en base de datos
            var exist = await _authorizationRequestRepository.GetOne(authorizationRequest.TransactionId);
            if (exist != null)
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;
            }   
            await _authorizationRequestRepository.Save(authorizationRequest);

            if (statusProcessPayment == "Approved") 
            {
                //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas
                var approved = new AuthorizationRequestApproved()
                {
                    Id = Guid.Empty,
                    Amount = authorizationRequest.Amount,
                    TransactionDate = authorizationRequest.TransactionDate,
                    CustomerName = authorizationRequest.CustomerName,
                    TransactionId = authorizationRequest.TransactionId,
                };
                await SendMessage("authorization-request", JsonSerializer.Serialize(approved));
            }

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeDevolucion(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();

            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequest = new AuthorizationRequest
            {
                Id = Guid.Empty,
                TransactionId = model.TransactionId,
                TransactionDate = model.TransactionDate,
                Amount = model.Amount,
                Status = statusProcessPayment,
                CustomerName = model.CustomerName,
                CustomerType = model.CustomerType,
                CreatedAt = DateTime.Now,
                TransactionType = model.TransactionType
            };


            //Guardar auth en base de datos
            var exist = await _authorizationRequestRepository.GetOne(authorizationRequest.TransactionId);
            if (exist != null)
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;
            }
            await _authorizationRequestRepository.Save(authorizationRequest);

            if (statusProcessPayment == "Approved")
            {
                //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas
                var approved = new AuthorizationRequestApproved()
                {
                    Id = Guid.Empty,
                    Amount = authorizationRequest.Amount,
                    TransactionDate = authorizationRequest.TransactionDate,
                    CustomerName = authorizationRequest.CustomerName,
                    TransactionId = authorizationRequest.TransactionId,
                };
                await SendMessage("authorization-request", JsonSerializer.Serialize(approved));
            }

            return result;
        }

        public  async Task<AuthorizationRequestResponseDto> TransactionTypeReversa(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();

            result.Message = $"Payment {statusProcessPayment}"; ;
            var authorizationRequest = new AuthorizationRequest
            {
                Id = Guid.Empty,
                TransactionId = model.TransactionId,
                TransactionDate = model.TransactionDate,
                Amount = model.Amount,
                Status = statusProcessPayment,
                CustomerName = model.CustomerName,
                CustomerType = model.CustomerType,
                CreatedAt = DateTime.Now,
                TransactionType = model.TransactionType
            };

           
            //Guardar auth en base de datos
            var exist = await _authorizationRequestRepository.GetOne(authorizationRequest.TransactionId);
            if (exist != null)
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;
            }
            await _authorizationRequestRepository.Save(authorizationRequest);

            if (statusProcessPayment == "Approved")
            {
                //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas
                var approved = new AuthorizationRequestApproved()
                {
                    Id = Guid.Empty,
                    Amount = authorizationRequest.Amount,
                    TransactionDate = authorizationRequest.TransactionDate,
                    CustomerName = authorizationRequest.CustomerName,
                    TransactionId = authorizationRequest.TransactionId,
                };
                await SendMessage("authorization-request", JsonSerializer.Serialize(approved));
            }

            return result;
        }

    }
}
