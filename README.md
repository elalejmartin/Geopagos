INSTALACION
Se necesita docker para levantar los microservicios
La base de datos se crea solo a travez de una migracion al levantar los microservicios

ARQUITECTURA

![image](https://github.com/user-attachments/assets/f600d03b-b241-4946-bf42-9beaa8ee857f)





ARQUITECTURA
El proyecto cuenta con tres Microservicios
1. services-authorization: Este microservicio de almacenar y gestionar transacciones.
2. services-payment-procesor: Este microservicio se encarga de validar si una transaccion esta aprobada
3. services-authorization-cron: Este microservicio es un cron que se ejecuta cada un minuto para validar si hay transacciones del tipo 2 que pasaron los 5 minutos de espera

Por cuestiones de tiempo se uso solo una base de datos para guardar toda la informacion.
Se utilizo arquitectura limpia para el desarrollo del microservicio services-authorization, los otros dos microservicios se simplifico el desarrollo por cuestiones de tiempo.

Se utilizo Rabbitmq como cola de mensajeria
Se utilizo ocelot como api gateway
Se utilizo consul como discovery de los microservicios
Se utilizo sql server como base de datos principal.

USO
Probar con postman o directamente entrando a swagger
SWAGGER: http://localhost:8002/swagger/index.html

POSTMAN: 
URL:http://localhost:8002/api/authorizationRequests/
METHOD: POST
BODY: Este es un ejemplo 
{
  "transactionId": "65456456", //Para el customertype 1 son unicos, para el 2 se repite con la confirmacion
  "transactionDate": "2025-01-19T21:45:32.988Z",
  "amount": 100,
  "customerName": "Santander",
  "customerType": "1", //1 Normal, 2 Con confirmacion
  "transactionType": "Cobro" // Solo se admite Cobro,Devolucion,Reversa,Confirmacion para customerType:2, para customerType:1 Cobro,Devolucion,Reversa
}
