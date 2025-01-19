using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.Entities
{
    public class AuthorizationRequestApproved
    {
        public Guid Id { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? Amount { get; set; }
        public string? CustomerName { get; set; }
  

    }
}
