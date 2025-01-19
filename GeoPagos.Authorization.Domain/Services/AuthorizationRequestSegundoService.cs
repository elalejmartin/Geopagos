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
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GeoPagos.Authorization.Domain.Services
{
    public class AuthorizationRequestSegundoService : AuthorizacionRequestBase, IAuthorizationRequestService
    {
        private readonly IAuthorizationRequestRepository _authorizationRequestRepository;
        private readonly ILogger<AuthorizationRequestSegundoService> _logger;
        private readonly HttpClient _client;
        public AuthorizationRequestSegundoService(IAuthorizationRequestRepository authorizationRequestRepository
            , ILogger<AuthorizationRequestSegundoService> logger) : base(authorizationRequestRepository)    
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
                switch (model.TransactionType)
                {
                    case "Cobro":
                        result = await TransactionTypeCobro(model);
                        break;
                    case "Devolucion":
                        result = await TransactionTypeDevolucion(model);
                        break;
                    case "Reversa":
                        result = await TransactionTypeReversa(model);
                        break;
                    case "Confirmacion":
                        result = await TransactionTypeConfirmacion(model);
                        break;
                    default:
                        break;
                }

            }
            else
            {
                result.Message = $"Payment rejected: {verify.Error}";
            }


            return result;  
        }


        public async Task<AuthorizationRequestResponseDto> TransactionTypeCobro(AuthorizationRequestDto model)
        {
            var result = new AuthorizationRequestResponseDto();

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

            //Guardar auth en base de datos
            await _authorizationRequestRepository.Save(authorizationRequest);

            //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeDevolucion(AuthorizationRequestDto model)
        {
            var result = new AuthorizationRequestResponseDto();

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

            //Guardar auth en base de datos
            await _authorizationRequestRepository.Save(authorizationRequest);

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeReversa(AuthorizationRequestDto model)
        {
            var result = new AuthorizationRequestResponseDto();

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

            //Guardar auth en base de datos
            await _authorizationRequestRepository.Save(authorizationRequest);

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeConfirmacion(AuthorizationRequestDto model)
        {
            var result = new AuthorizationRequestResponseDto();


            var exist = await _authorizationRequestRepository.GetOne(model.TransactionId);  
            if(exist==null)
            {
                result.Message = "Transaction not exist";
                return result;
            }

            if (DateTime.Now.Subtract(exist.TransactionDate.Value) > TimeSpan.FromMinutes(5)) 
            {
                result.Message = "Transaction expired";
                return result;  
            }

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

            //Guardar auth en base de datos
            await _authorizationRequestRepository.Save(authorizationRequest);

            return result;
        }

    }
}
