using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Infraestructure.Repositories
{
    public class AuthorizationRequestApprovedRepository : IAuthorizationRequestApprovedRepository
    {
        private readonly IServiceScopeFactory _serviceProvider;
        //private readonly ApplicationDbContext _context;
        public AuthorizationRequestApprovedRepository(
             IServiceScopeFactory serviceProvider
            //ApplicationDbContext applicationDbContext
            )
        {
            //_context = applicationDbContext;
            _serviceProvider = serviceProvider;
        }

        public async Task Save(AuthorizationRequestApproved entity)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var _context =  scope.ServiceProvider.GetService<ApplicationDbContext>();
            if (entity.Id == Guid.Empty)
            {
                _context.AuthorizationRequestApproved.Add(entity);

            }
            else
            {
                _context.AuthorizationRequestApproved.Update(entity);
            }
            await _context.SaveChangesAsync();
        }
    }
}
