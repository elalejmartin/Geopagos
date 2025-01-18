using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;


namespace GeoPagos.Authorization.Domain.Services
{
    public class AuthorizationRequestService : IAuthorizationRequestService
    {
        private readonly IAuthorizationRequestRepository _authorizationRequestRepository;   
        public AuthorizationRequestService(IAuthorizationRequestRepository authorizationRequestRepository) 
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
