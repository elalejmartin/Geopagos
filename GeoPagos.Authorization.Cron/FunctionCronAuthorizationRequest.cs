using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GeoPagos.Authorization.Cron.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GeoPagos.Authorization.Cron
{
    public class FunctionCronAuthorizationRequest
    {
        [FunctionName("FunctionCronAuthorizationRequest")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

                await QueryPendings(log);
            }
            catch (Exception ex)
            {
                log.LogError($"Error execute cron {ex.Message}");
            }

        }

        private async Task QueryPendings(ILogger log) 
        {
            log.LogInformation($"Started Extraction.");
            // Obtener la cadena de conexión desde la configuración
            var connectionString = Environment.GetEnvironmentVariable("SQLConnectionString");

            // Usar Dapper para ejecutar la consulta
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Realizar una consulta usando Dapper
                //var sqlQuery = @"
                //            SELECT *
                //            FROM AuthorizationRequest
                //            WHERE TransactionDate >= DATEADD(MINUTE, -5, GETDATE());";

                var sqlQuery = @"
                            SELECT *
                            FROM AuthorizationRequest";

                var authorizations = await connection.QueryAsync<AuthorizationRequestDto>(sqlQuery);


                foreach (var item in authorizations)
                {
                    await SendAuthorizationRequests(log, item);   
                }


            }
            log.LogInformation($"End Extraction.");
        }

        public async Task SendAuthorizationRequests(ILogger log,AuthorizationRequestDto model) 
        {
            log.LogInformation($"Start Send Revesa.");

            // URL de la API externa
            string apiUrl = Environment.GetEnvironmentVariable("UrlAppService") + "api/AuthorizationRequests/";


            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(10);

            string jsonContent = JsonSerializer.Serialize(model, new JsonSerializerOptions
            {
                WriteIndented = true // Formato con sangría para mayor legibilidad
            });


            // Crea el contenido HTTP (cuerpo de la solicitud)
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                log.LogInformation($"Reversa success send. {responseContent}");
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                log.LogError($"Error creation Reversa: {responseContent}");
            
                //return new StatusCodeResult(500); // Código 500
            }
            log.LogInformation($"End Send Revesa.");
        }
    }
}
