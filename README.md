# 🚀 **Guía de Instalación y Uso**

## 📦 **INSTALACIÓN**
1. Asegúrate de tener **Docker** instalado para levantar los microservicios.
2. La base de datos se crea automáticamente al ejecutar una migración durante el arranque de los microservicios.

---

## 🏛️ **ARQUITECTURA**

![Arquitectura del Proyecto](https://github.com/user-attachments/assets/f600d03b-b241-4946-bf42-9beaa8ee857f)

### Descripción
El proyecto consta de **tres microservicios** principales:
1. **services-authorization**: 
   - Gestor de almacenamiento y transacciones.
2. **services-payment-procesor**: 
   - Valida si una transacción está aprobada.
3. **services-authorization-cron**: 
   - Cron job que se ejecuta cada minuto para validar transacciones del tipo 2 que superen los 5 minutos de espera.

### Tecnologías Utilizadas
- **Base de datos**: 
  - Por simplicidad, se usó una única base de datos para almacenar toda la información.
- **Arquitectura limpia**: 
  - Implementada en el microservicio `services-authorization`. Los otros dos microservicios tienen una implementación más sencilla por razones de tiempo.
- **RabbitMQ**: 
  - Cola de mensajería.
- **Ocelot**: 
  - API Gateway.
- **Consul**: 
  - Discovery de microservicios.
- **SQL Server**: 
  - Base de datos principal.

---

## 🔧 **USO**

Puedes probar el sistema utilizando **Postman** o directamente en **Swagger**.

**CustomerType:**
-Tipo 1: Recibe transaccion y verifica si la aprueba o no
-Tipo 2: Recibe transaccion y luego su confirmacion

**TransactionType:**
-Para tipo 1 admite Cobro,Devolucion,Reversa 
-Para tipo 2 admite Cobro,Devolucion,Reversa,Confirmacion 

### **Swagger**
- Accede a la documentación interactiva:
  [http://localhost:8002/swagger/index.html](http://localhost:8002/swagger/index.html)

### **Postman**
#### Endpoint: 
```http
POST http://localhost:8002/api/authorizationRequests/

{
  "transactionId": "65456456", 
  "transactionDate": "2025-01-19T21:45:32.988Z",
  "amount": 100,
  "customerName": "Santander",
  "customerType": "1", 
  "transactionType": "Cobro"
}

