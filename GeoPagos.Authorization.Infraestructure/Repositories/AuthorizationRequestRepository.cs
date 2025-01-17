﻿using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Infraestructure.Repositories
{

    public class AuthorizationRequestRepository : IAuthorizationRequestRepository
    {
        //private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;
        public AuthorizationRequestRepository(
           // IServiceProvider serviceProvider
            ApplicationDbContext applicationDbContext
            )
        {
            _context = applicationDbContext;
            //_serviceProvider = serviceProvider;
        }

        public async Task Save(AuthorizationRequest entity)
        {
            //var _context = _serviceProvider.GetService<ApplicationDbContext>();
            if (entity.Id == Guid.Empty)
            {
                _context.AuthorizationRequest.Add(entity);

            }
            else
            {
                _context.AuthorizationRequest.Update(entity);
            }
            await _context.SaveChangesAsync();
        }



        public async Task<AuthorizationRequest> GetOne(string transactionId)
        {
            //var _context = _serviceProvider.GetService<ApplicationDbContext>();
            var entity = await _context.AuthorizationRequest.Where(a=>a.TransactionId== transactionId).AsNoTracking().FirstOrDefaultAsync();
            return entity;  
        }

        public async Task<AuthorizationRequest> GetOneByStatus(string transactionId, string status) 
        {
            var entity = await _context.AuthorizationRequest.Where(a => a.TransactionId == transactionId && a.Status== status).AsNoTracking().FirstOrDefaultAsync();
            return entity;
        }
    }
}
