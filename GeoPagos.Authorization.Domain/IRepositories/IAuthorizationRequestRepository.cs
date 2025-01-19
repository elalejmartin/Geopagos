using GeoPagos.Authorization.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.IRepositories
{
    public interface IAuthorizationRequestRepository
    {
        Task Save(AuthorizationRequest entity);
        Task<AuthorizationRequest> GetOne(string transactionId);
    }
}
