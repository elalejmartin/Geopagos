﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Application.DTOs
{
    public class PaymentDto
    {
        public decimal? Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? CustomerId { get; set; }
        public string? Response { get; set; }
        public string? Error { get; set; }   
    }
}