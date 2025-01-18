using Consul;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Tracing;

var builder = WebApplication.CreateBuilder(args);

// Configuración de OpenTelemetry
//builder.Services.AddOpenTelemetry()
//    .WithTracing(tracerProviderBuilder =>
//    {
//        tracerProviderBuilder
//            .AddAspNetCoreInstrumentation()  // Instrumentar ASP.NET Core
//            .AddHttpClientInstrumentation() // Instrumentar HttpClient
//            .AddJaegerExporter(options =>
//            {
//                options.AgentHost = "services-jaeger"; // Dirección del agente Jaeger
//                options.AgentPort = 14268;        // Puerto del agente Jaeger
//                //options.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.UdpCompactThrift;
//                //options.ExportProcessorType = ExportProcessorType.Batch; // Exportación por lotes
//            })
//            .SetResourceBuilder(
//                ResourceBuilder.CreateDefault()
//                    .AddService("Gateway") // Nombre del servicio
//            );
//    });

// Cargar el archivo `ocelot.json` explícitamente
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configura Ocelot con la configuración leída desde `ocelot.json`
builder.Services.AddOcelot(builder.Configuration);

// Configura Swagger para Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration)
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "API Gateway con Ocelot",
            Version = "v1"
        });
    });


// Configurar OpenTelemetry con Jaeger
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("MyDotNetApp")) // Nombre del servicio en Jaeger
            .AddAspNetCoreInstrumentation() // Trazado automático de ASP.NET Core
            .AddHttpClientInstrumentation() // Trazado automático de HttpClient
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "services-jaeger"; // Dirección del agente Jaeger
                options.AgentPort = 6831;        // Puerto del agente Jaeger
            });
    });



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(TracerProvider.Default);





var app = builder.Build();



// Configurar Consul
var consulClient = new ConsulClient(config =>
{
    config.Address = new Uri("http://services-consul:8500"); // Dirección de Consul
});

// Configurar el registro del servicio
var registration = new AgentServiceRegistration
{
    ID = "gateway-service-1",  // ID único para esta instancia del servicio
    Name = "Gateway-Service",  // Nombre del servicio
    Address = "localhost",    // Dirección del servicio
    Port = 44368               // Puerto donde corre el servicio
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

app.UseOcelot();

app.Run();
