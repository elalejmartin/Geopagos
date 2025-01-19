﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Application.DTOs
{
    public class AuthorizationRequestDto
    {
        [Required]
        public string? TransactionId { get; set; }
        [Required]
        public DateTime? TransactionDate { get; set; }
        [Required]
        public decimal? Amount { get; set; }
        [Required]
        public string? CustomerName { get; set; }
        [Required]
        public string? CustomerType { get; set; }
        [Required]
        public string? TransactionType { get; set; }
    }
}
