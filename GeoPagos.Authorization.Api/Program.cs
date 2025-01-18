using Consul;
using GeoPagos.Authorization.Infraestructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

// Add services to the container.

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
    dbContext.Database.Migrate(); // Esto aplicará las migraciones automáticamente
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
