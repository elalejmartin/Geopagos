﻿using GeoPagos.Authorization.Application.DTOs;
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

namespace GeoPagos.Authorization.Domain.Services.AuthorizationRequest
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

            _logger.LogInformation($"Started Authorization");
            var verify = await VerifyAmountPayment(model);
            _logger.LogInformation($"VerifyAmountPayment: {verify.Response}");

            if (verify.Response == "Approved")
            {
                switch (model.TransactionType)
                {
                    case "Cobro":
                        result = await TransactionTypeCobro(model, "Pending");
                        break;
                    case "Devolucion":
                        result = await TransactionTypeDevolucion(model, "Pending");
                        break;
                    case "Reversa":
                        result = await TransactionTypeReversa(model, "Pending");
                        break;
                    case "Confirmacion":
                        result = await TransactionTypeConfirmacion(model, verify.Response);
                        break;
                    default:
                        result.Message = $"Invalid TransactionType: {model.TransactionType} - Solo se admite Cobro,Devolucion,Reversa,Confirmacion para customerType:2";
                        break;
                }
            }
            else 
            {
                result.Message = $"Payment {verify.Response}";  
            }


            _logger.LogInformation($"End Authorization");


            return result;
        }


        public async Task<AuthorizationRequestResponseDto> TransactionTypeCobro(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();


            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequest = new Entities.AuthorizationRequest
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

            var exist = await _authorizationRequestRepository.GetOne(authorizationRequest.TransactionId);
            if (exist != null)
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;
            }

            //Guardar auth en base de datos
            await _authorizationRequestRepository.Save(authorizationRequest);

            //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeDevolucion(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();


            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequest = new Entities.AuthorizationRequest
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
            await _authorizationRequestRepository.Save(authorizationRequest);

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeReversa(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();

            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequest = new Entities.AuthorizationRequest
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
            await _authorizationRequestRepository.Save(authorizationRequest);

            return result;
        }

        public async Task<AuthorizationRequestResponseDto> TransactionTypeConfirmacion(AuthorizationRequestDto model, string statusProcessPayment)
        {
            var result = new AuthorizationRequestResponseDto();


            var existApproved = await _authorizationRequestRepository.GetOneByStatus(model.TransactionId, "Approved");
            if (existApproved != null ) 
            {
                result.Message = "Payment rejected: TransactionId already exists";
                return result;  
            }

            var exist = await _authorizationRequestRepository.GetOneByStatus(model.TransactionId, "Pending");
            if (exist == null)
            {
                result.Message = "Transaction pending not exist";
                return result;
            }

            if (model.TransactionDate.Value.Subtract(exist.TransactionDate.Value) > TimeSpan.FromMinutes(5))
            {
                result.Message = "Transaction expired";
                return result;
            }

            result.Message = $"Payment {statusProcessPayment}";
            var authorizationRequest = new Entities.AuthorizationRequest
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
            await _authorizationRequestRepository.Save(authorizationRequest);

            if (statusProcessPayment == "Approved")
            {
                //Enviar a la cola de mensajes  rabbitmq solo para transacciones confirmadas
                var approved = new Application.DTOs.AuthorizationRequestApprovedDto()
                {             
                    Amount = authorizationRequest.Amount,
                    TransactionDate = authorizationRequest.TransactionDate,
                    CustomerName = authorizationRequest.CustomerName,
                    TransactionId = authorizationRequest.TransactionId,
                };
                await SendMessage("authorization-request", JsonSerializer.Serialize(approved));
                _logger.LogInformation($"Mensaje enviado a la cola: {JsonSerializer.Serialize(approved)}");
            }

            return result;
        }

    }
}
