using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;


namespace GeoPagos.Authorization.Domain.Services
{
    public  class AuthorizationRequestFactory : IAuthorizationRequestFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthorizationRequestFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public  IAuthorizationRequestService GetAuthorizationRequest(string tipo)
        {
      
            return tipo switch
            {
                "1" => _serviceProvider.GetRequiredService<AuthorizationRequestPrimeroService>(),
                "2" => _serviceProvider.GetRequiredService<AuthorizationRequestSegundoService>(),
                _ => throw new ArgumentException("Tipo no válido."),
            };
        }
    }
}
