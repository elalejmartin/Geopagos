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
    .AddConsul(); 

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();



//app.UseAuthorization();

app.MapControllers();


app.UseOcelot().Wait();

app.Run();
