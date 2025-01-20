using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.Services.AuthorizationRequestApproved
{
    public class AuthorizationRequesApprovedService : IAuthorizationRequestApprovedService
    {
        private readonly IAuthorizationRequestApprovedRepository _authorizationRequestRepository;
        private readonly ILogger<AuthorizationRequesApprovedService> _logger;
        public AuthorizationRequesApprovedService(IAuthorizationRequestApprovedRepository authorizationRequestRepository, ILogger<AuthorizationRequesApprovedService> logger)
        {
            _authorizationRequestRepository = authorizationRequestRepository;
            _logger = logger;
        }

        public async Task SaveList(List<AuthorizationRequestApprovedDto> list)
        {
            try
            {
                foreach (var item in list)
                {
                    var entity = new Entities.AuthorizationRequestApproved
                    {
                        Id = Guid.Empty,    
                        TransactionId = item.TransactionId,
                        TransactionDate = item.TransactionDate,
                        Amount = item.Amount,
                        CustomerName = item.CustomerName,
     
                    };
                    await _authorizationRequestRepository.Save(entity);

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
  
    }
}
