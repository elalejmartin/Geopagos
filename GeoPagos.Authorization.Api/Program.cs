using Consul;
using GeoPagos.Authorization.Infraestructure;
using Microsoft.EntityFrameworkCore;
using Serilog.Sinks.Elasticsearch;
using Serilog;
using GeoPagos.Authorization.Application.Interfaces;
using GeoPagos.Authorization.Domain.Services;
using GeoPagos.Authorization.Domain.IRepositories;
using GeoPagos.Authorization.Infraestructure.Repositories;
using GeoPagos.Authorization.Api.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

// Configuración de Serilog
//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.Console()  // Si deseas también mostrar los logs en la consola
//    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))  // Cambia la URL si tu servidor de Elasticsearch está en otro lugar
//    {
//        AutoRegisterTemplate = true,  // Esto permite que Serilog registre automáticamente el template del índice
//        IndexFormat = "authorization-services-logs-{0:yyyy.MM.dd}"  // El formato de los índices, puedes ajustarlo según prefieras
//    })
//    .CreateLogger();

////builder.Host.UseSerilog();
//builder.Host.UseSerilog(Log.Logger);


// Add services to the container.

builder.Services.AddScoped<IAuthorizationRequestFactory, AuthorizationRequestFactory>();
builder.Services.AddScoped<IAuthorizationRequestService, AuthorizationRequestPrimeroService>();
builder.Services.AddScoped<IAuthorizationRequestService, AuthorizationRequestSegundoService>();
builder.Services.AddScoped<IAuthorizationRequestRepository, AuthorizationRequestRepository>();
builder.Services.AddScoped<AuthorizationRequestPrimeroService>();
builder.Services.AddScoped<AuthorizationRequestSegundoService>();
builder.Services.AddHostedService<AuthorizationRequestConsumer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar Consul
var consulClient = new ConsulClient(config =>
{
    config.Address = new Uri("http://services-consul:8500"); // Dirección de Consul
});

// Configurar el registro del servicio
var registration = new AgentServiceRegistration
{
    ID = "authorization-service-1",  // ID único para esta instancia del servicio
    Name = "Authorization-Service",  // Nombre del servicio
    Address = "localhost",    // Dirección del servicio
    Port = 50162               // Puerto donde corre el servicio
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

// Aplicar las migraciones al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Aplica las migraciones
        //System.Threading.Thread.Sleep(10000);
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {

    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
