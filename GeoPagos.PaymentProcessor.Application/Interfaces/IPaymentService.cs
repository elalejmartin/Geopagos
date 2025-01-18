using GeoPagos.PaymentProcessor.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.PaymentProcessor.Application.Interfaces
{
    public interface IPaymentService
    {
        string VerifyAmount(PaymentDto model);
    }
}
