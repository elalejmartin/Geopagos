﻿using GeoPagos.Authorization.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Application.Interfaces
{
    public interface IAuthorizationRequestApprovedService
    {
        Task SaveList(List<AuthorizationRequestApprovedDto> list);
    }
}