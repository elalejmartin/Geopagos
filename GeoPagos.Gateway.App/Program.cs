using Consul;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Tracing;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de OpenTelemetry
//builder.Services.AddOpenTelemetry()
//    .WithTracing(tracerProviderBuilder =>
//    {
//        tracerProviderBuilder
//            .AddAspNetCoreInstrumentation()  // Instrumentar ASP.NET Core
//            .AddHttpClientInstrumentation() // Instrumentar HttpClient
//            .AddJaegerExporter(options =>
//            {
//                options.AgentHost = "services-jaeger"; // Direcci�n del agente Jaeger
//                options.AgentPort = 14268;        // Puerto del agente Jaeger
//                //options.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.UdpCompactThrift;
//                //options.ExportProcessorType = ExportProcessorType.Batch; // Exportaci�n por lotes
//            })
//            .SetResourceBuilder(
//                ResourceBuilder.CreateDefault()
//                    .AddService("Gateway") // Nombre del servicio
//            );
//    });

// Cargar el archivo `ocelot.json` expl�citamente
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configura Ocelot con la configuraci�n le�da desde `ocelot.json`
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
            .AddAspNetCoreInstrumentation() // Trazado autom�tico de ASP.NET Core
            .AddHttpClientInstrumentation() // Trazado autom�tico de HttpClient
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "services-jaeger"; // Direcci�n del agente Jaeger
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
    config.Address = new Uri("http://services-consul:8500"); // Direcci�n de Consul
});

// Configurar el registro del servicio
var registration = new AgentServiceRegistration
{
    ID = "gateway-service-1",  // ID �nico para esta instancia del servicio
    Name = "Gateway-Service",  // Nombre del servicio
    Address = "localhost",    // Direcci�n del servicio
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
