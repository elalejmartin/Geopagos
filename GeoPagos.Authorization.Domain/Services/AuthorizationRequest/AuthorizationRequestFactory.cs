using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace GeoPagos.Authorization.Domain.Services.AuthorizationRequest
{
    public class AuthorizationRequestFactory : IAuthorizationRequestFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthorizationRequestFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAuthorizationRequestService GetAuthorizationRequest(string tipo)
        {
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    return tipo switch
            //    {
            //        "1" => scope.ServiceProvider.GetRequiredService<AuthorizationRequestPrimeroService>(),
            //        "2" => scope.ServiceProvider.GetRequiredService<AuthorizationRequestSegundoService>(),
            //        _ => throw new ArgumentException("Tipo no válido."),
            //    };
            //}
            return tipo switch
            {
                "1" => _serviceProvider.GetRequiredService<AuthorizationRequestPrimeroService>(),
                "2" => _serviceProvider.GetRequiredService<AuthorizationRequestSegundoService>(),
                _ => throw new ArgumentException($"Customer Type: {tipo}-Solo se admite 1 o 2"),
            };
        }
    }
}
