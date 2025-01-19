using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Application.DTOs
{
    public class AuthorizationRequestDto
    { 
        public string? TransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? Amount { get; set; }    
        public string? CustomerName { get; set; }
        public string? CustomerType { get; set; } 
        public string? TransactionType { get; set; }
    }
}
