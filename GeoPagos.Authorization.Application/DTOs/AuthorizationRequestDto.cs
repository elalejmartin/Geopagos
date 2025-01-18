﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Application.DTOs
{
    public class AuthorizationRequestDto
    {
        public Guid Id { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerType { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
