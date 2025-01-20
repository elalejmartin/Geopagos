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

namespace GeoPagos.Authorization.Domain.Services.AuthorizationRequest
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
            _logger.LogInformation($"Started Authorization");

            var verify = await VerifyAmountPayment(model);
            _logger.LogInformation($"VerifyAmountPayment: {verify.Response}");

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
            _logger.LogInformation($"End Authorization");

            return result;
        }
 

        public async Task<AuthorizationRequestResponseDto> TransactionTypeCobro(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();

            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequestEntity = new GeoPagos.Authorization.Domain.Entities.AuthorizationRequest
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
            var exist = await _authorizationRequestRepository.GetOne(authorizationRequestEntity.TransactionId);
            if (exist != null)
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;
            }
            await _authorizationRequestRepository.Save(authorizationRequestEntity);

            if (statusProcessPayment == "Approved")
            {
                //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas
                var approved = new Application.DTOs.AuthorizationRequestApprovedDto()
                {                   
                    Amount = authorizationRequestEntity.Amount,
                    TransactionDate = authorizationRequestEntity.TransactionDate,
                    CustomerName = authorizationRequestEntity.CustomerName,
                    TransactionId = authorizationRequestEntity.TransactionId,
                };
                await SendMessage("authorization-request", JsonSerializer.Serialize(approved));
                _logger.LogInformation($"Mensaje enviado a la cola: {JsonSerializer.Serialize(approved)}");
            }

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeDevolucion(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();

            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequestEntity = new GeoPagos.Authorization.Domain.Entities.AuthorizationRequest
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
            var exist = await _authorizationRequestRepository.GetOne(authorizationRequestEntity.TransactionId);
            if (exist != null)
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;
            }
            await _authorizationRequestRepository.Save(authorizationRequestEntity);

            if (statusProcessPayment == "Approved")
            {
                //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas
                var approved = new Application.DTOs.AuthorizationRequestApprovedDto()
                {                 
                    Amount = authorizationRequestEntity.Amount,
                    TransactionDate = authorizationRequestEntity.TransactionDate,
                    CustomerName = authorizationRequestEntity.CustomerName,
                    TransactionId = authorizationRequestEntity.TransactionId,
                };
                await SendMessage("authorization-request", JsonSerializer.Serialize(approved));
                _logger.LogInformation($"Mensaje enviado a la cola: {JsonSerializer.Serialize(approved)}");
            }

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeReversa(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();

            result.Message = $"Payment {statusProcessPayment}"; ;
            var authorizationRequestEntity = new GeoPagos.Authorization.Domain.Entities.AuthorizationRequest
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
            var exist = await _authorizationRequestRepository.GetOne(authorizationRequestEntity.TransactionId);
            if (exist != null)
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;
            }
            await _authorizationRequestRepository.Save(authorizationRequestEntity);

            if (statusProcessPayment == "Approved")
            {
                //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas
                var approved = new Application.DTOs.AuthorizationRequestApprovedDto()
                {                 
                    Amount = authorizationRequestEntity.Amount,
                    TransactionDate = authorizationRequestEntity.TransactionDate,
                    CustomerName = authorizationRequestEntity.CustomerName,
                    TransactionId = authorizationRequestEntity.TransactionId,
                };
                await SendMessage("authorization-request", JsonSerializer.Serialize(approved));
                _logger.LogInformation($"Mensaje enviado a la cola: {JsonSerializer.Serialize(approved)}");
            }

            return result;
        }

    }
}
