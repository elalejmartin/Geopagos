using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Infraestructure.Repositories
{

    public class AuthorizationRequestRepository : IAuthorizationRequestRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public AuthorizationRequestRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task Save(AuthorizationRequest entity)
        {
            if (entity.Id == Guid.Empty)
            {
                _applicationDbContext.AuthorizationRequest.Add(entity);

            }
            else
            {
                _applicationDbContext.AuthorizationRequest.Update(entity);
            }
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
