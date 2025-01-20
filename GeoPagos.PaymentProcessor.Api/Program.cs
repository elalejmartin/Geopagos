using Consul;
using GeoPagos.PaymentProcessor.Application.Interfaces;
using GeoPagos.PaymentProcessor.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IPaymentService, PaymentService>();

var app = builder.Build();

// Configurar Consul
var consulClient = new ConsulClient(config =>
{
    config.Address = new Uri("http://services-consul:8500"); // Dirección de Consul
});

// Configurar el registro del servicio
var registration = new AgentServiceRegistration
{
    ID = "payment-processor-service-1",  // ID único para esta instancia del servicio
    Name = "Payment-Processor-Service",  // Nombre del servicio
    Address = "services-payment-processor ",    // Dirección del servicio
    Port = 8003               // Puerto donde corre el servicio
};

// Registrar el servicio en Consul
await consulClient.Agent.ServiceRegister(registration);
// Registrar el servicio en Consul
//consulClient.Agent.ServiceRegister(registration).Wait();

// Manejador para anular el registro cuando el servicio se apaga
app.Lifetime.ApplicationStopping.Register(async () =>
{
    await consulClient.Agent.ServiceDeregister(registration.ID);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
