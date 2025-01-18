using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.Entities
{
    public class AuthorizationRequest
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get;  set; }
        public string CustomerId { get; set; }
        public string CustomerType { get; set; }
        public DateTime CreatedAt { get; set; }

        public void Approve()
        {
            Status = "Approved";
        }

        public void Reject()
        {
            Status = "Rejected";
        }
    }
}
