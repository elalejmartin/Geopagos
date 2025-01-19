using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.PaymentProcessor.Application.DTOs
{
    public class PaymentDto
    {
        public decimal? Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? CustomerName { get; set; }  
        public string? Response { get; set; }    
    }
}
