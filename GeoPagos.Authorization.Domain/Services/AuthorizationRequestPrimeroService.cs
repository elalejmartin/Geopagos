using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.Services
{
    public class AuthorizationRequestPrimeroService : IAuthorizationRequestService
    {
        private readonly IAuthorizationRequestRepository _authorizationRequestRepository;
        public AuthorizationRequestPrimeroService(IAuthorizationRequestRepository authorizationRequestRepository)
        {
            _authorizationRequestRepository = authorizationRequestRepository;
        }

        public async Task Authorize(AuthorizationRequestDto model)
        {
            var entity = new AuthorizationRequest();
            await _authorizationRequestRepository.Save(entity);
        }
    }
}
