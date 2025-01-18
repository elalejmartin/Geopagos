using GeoPagos.PaymentProcessor.Application.DTOs;
using GeoPagos.PaymentProcessor.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoPagos.PaymentProcessor.Domain.Services
{
    public class PaymentService : IPaymentService
    {
        public PaymentService() 
        {
        }
        public string VerifyAmount(PaymentDto model)
        {
            // Verifica si el número tiene decimales
            if (model.Amount % 1 == 0)
            {
                return model.Response ="Approved";
            }
            else
            {
                return model.Response = "No Approved";
            }
        }   
    }
}
