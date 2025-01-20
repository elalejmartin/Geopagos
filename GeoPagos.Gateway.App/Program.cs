using Consul;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Values;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Tracing;

var builder = WebApplication.CreateBuilder(args);

// Cargar el archivo `ocelot.json` explícitamente
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configura Ocelot con la configuración leída desde `ocelot.json`
//builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddOcelot()
    //.AddConfigStoredInConsul(); // !
    .AddConsul(); // Añadir Consul como proveedor de descubrimiento

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();



// Configurar Consul
//var consulClient = new ConsulClient(config =>
//{
//    config.Address = new Uri("http://services-consul:8500"); // Dirección de Consul
//});

//// Configurar el registro del servicio
//var registration = new AgentServiceRegistration
//{
//    ID = "gateway-service-1",  // ID único para esta instancia del servicio
//    Name = "Gateway-Service",  // Nombre del servicio
//    Address = "services-gateway",    // Dirección del servicio
//    Port = 8001               // Puerto donde corre el servicio
//};

//// Registrar el servicio en Consul
//await consulClient.Agent.ServiceRegister(registration);
//// Registrar el servicio en Consul
////consulClient.Agent.ServiceRegister(registration).Wait();

//// Manejador para anular el registro cuando el servicio se apaga
//app.Lifetime.ApplicationStopping.Register(async () =>
//{
//    await consulClient.Agent.ServiceDeregister(registration.ID);
//});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();



//app.UseAuthorization();

app.MapControllers();

//app.UseOcelot();
app.UseOcelot().Wait();

app.Run();
