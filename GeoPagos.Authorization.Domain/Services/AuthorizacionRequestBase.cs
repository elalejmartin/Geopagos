using GeoPagos.Authorization.Application.DTOs;
using GeoPagos.Authorization.Domain.Entities;
using GeoPagos.Authorization.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GeoPagos.Authorization.Domain.Services
{
    public abstract class AuthorizacionRequestBase
    {
        private readonly HttpClient _client;
        private readonly IAuthorizationRequestRepository _authorizationRequestRepository;
        public AuthorizacionRequestBase(IAuthorizationRequestRepository authorizationRequestRepository)
        {
            _client = new HttpClient();
            _authorizationRequestRepository = authorizationRequestRepository;   
        }

        public virtual async Task<PaymentDto> VerifyAmountPayment(AuthorizationRequestDto model)
        {
            //var discovery = await GetServiceAddress("Payment-Processor-Service");
            // Crea el objeto JSON que deseas enviar en el cuerpo de la solicitud
            var data = new PaymentDto()
            {
                Amount = model.Amount,
                CustomerName = model.CustomerName,
                TransactionId = model.TransactionId
            };

            try
            {
                // URL del servicio al que quieres hacer la solicitud POST
                string url = $"http://services-payment-processor/api/payments/";  // Cambia por tu URL real


                string jsonContent = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true // Formato con sangría para mayor legibilidad
                });


                // Crea el contenido HTTP (cuerpo de la solicitud)
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Realiza la solicitud POST
                HttpResponseMessage response = await _client.PostAsync(url, content);

                // Si la respuesta es exitosa, lee el contenido
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    data.Response = responseContent;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    data.Error = responseContent;
                }
            }
            catch (Exception ex)
            {
                data.Error = ex.Message;
            }
            return data;
        }

       

    }
}
