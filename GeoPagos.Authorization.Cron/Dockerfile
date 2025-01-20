# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GeoPagos.Authorization.Cron/GeoPagos.Authorization.Cron.csproj", "GeoPagos.Authorization.Cron/"]
RUN dotnet restore "./GeoPagos.Authorization.Cron/GeoPagos.Authorization.Cron.csproj"
COPY . .
WORKDIR "/src/GeoPagos.Authorization.Cron"
RUN dotnet build "./GeoPagos.Authorization.Cron.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GeoPagos.Authorization.Cron.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true