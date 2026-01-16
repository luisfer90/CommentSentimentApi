# CommentSentiment API

API RESTful desarrollada en **ASP.NET Core (.NET 8)** para el análisis de sentimiento de comentarios de usuarios sobre productos. La API permite **crear comentarios**, **consultarlos con filtros**, y **obtener un resumen de sentimientos**, integrando **Google Gemini** como proveedor principal de análisis de sentimiento con un **mecanismo de respaldo basado en reglas**.

Este proyecto fue desarrollado como **prueba técnica Backend** y está completamente **dockerizado**.

---

## 🚀 Características principales

- API RESTful con ASP.NET Core
- Base de datos SQL Server
- Análisis de sentimiento:
  - **Proveedor principal:** Google Gemini API
  - **Fallback automático:** análisis basado en palabras clave
- Arquitectura en capas (Domain, Application, Infrastructure)
- Entity Framework Core + Migrations
- Docker + Docker Compose
- Swagger (OpenAPI)
- Pruebas automatizadas (unitarias e integración) para validar la lógica de negocio y los endpoints

---

## 🧱 Arquitectura del proyecto

```
RepositorioRaiz
│
├── CommentSentimentApi   <-- 📌 Carpeta donde se ejecuta Docker
│   ├── Application
│   │   ├── DTOs
│   │   │   ├── CreateCommentRequest.cs
│   │   │   ├── CommentResponse.cs
│   │   │   └── SentimentSummaryResponse.cs
│   │   ├── Interfaces
│   │   │   └── ISentimentAnalyzer.cs
│   │   └── Services
│   │       ├── GeminiSentimentAnalyzer.cs
│   │       └── RuleBasedSentimentAnalyzer.cs
│   │
│   ├── Controllers
│   │   └── CommentsController.cs
│   │
│   ├── Domain
│   │   └── Entities
│   │       └── Comment.cs
│   │
│   ├── Infrastructure
│   │   └── Data
│   │       └── AppDbContext.cs
│   │
│   ├── Migrations
│   ├── Dockerfile
│   ├── docker-compose.yml
│   └── Program.cs
│
└── CommentSentimentApi.sln
```

---

## 📦 Requisitos

- Docker
- Docker Compose

---

## 📁 Ubicación importante

⚠️ **Antes de ejecutar cualquier comando de Docker**, debes moverte a la carpeta:

```
cd CommentSentimentApi
```

Esto es necesario porque los archivos `Dockerfile` y `docker-compose.yml` se encuentran en esa carpeta y no en la raíz del repositorio.

---

## 🔑 Variables de entorno

### Google Gemini API Key

La API intenta **primero analizar el sentimiento usando Google Gemini**. Si ocurre cualquier error (API Key inválida, timeout, error HTTP, etc.), automáticamente se utiliza el **análisis basado en reglas** como respaldo.

Antes de levantar los contenedores, debes definir la variable de entorno:

#### Windows (PowerShell)
```powershell
$Env:GEMINI_API_KEY="AIzaSyXXXXXXXXXXXX"
```

#### Windows (CMD)
```cmd
set GEMINI_API_KEY=AIzaSyXXXXXXXXXXXX
```

#### Linux / macOS
```bash
export GEMINI_API_KEY=AIzaSyXXXXXXXXXXXX
```

---

## ▶️ Levantar el proyecto con Docker

Desde la carpeta `CommentSentimentApi`:

```bash
docker-compose up --build
```

Este comando levanta automáticamente:
- La API Backend
- La base de datos **SQL Server**

📌 **La base de datos se levanta automáticamente usando Docker Compose.**

---

## 🗄️ Base de datos y migraciones

No es necesario crear manualmente la tabla de la base de datos.

Al iniciar la aplicación, se ejecuta automáticamente la migración de Entity Framework Core gracias al siguiente código en `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

Esto asegura que:
- La base de datos exista
- Las tablas se creen automáticamente
- El esquema esté siempre sincronizado con el modelo

---

## 📖 Swagger

Una vez levantado el proyecto, puedes acceder a la documentación interactiva:

```
http://localhost:5000/swagger
```

---

## 🔌 Endpoints disponibles
Puerto default: 5000 (http)
### 1️⃣ Crear comentario

**POST** `/api/comments`

#### Request
```json
{
  "product_id": "PROD001",
  "user_id": "USER001",
  "comment_text": "Este producto es excelente, superó mis expectativas"
}
```

#### Response
```json
{
  "id": 1,
  "productId": "PROD001",
  "userId": "USER001",
  "commentText": "Este producto es excelente, superó mis expectativas",
  "sentiment": "positivo",
  "createdAt": "2026-01-15T23:45:00Z"
}
```

---

### 2️⃣ Obtener comentarios

**GET** `/api/comments`

#### Filtros opcionales
- `product_id`
- `sentiment`

#### Ejemplos
```
/api/comments
/api/comments?product_id=PROD001
/api/comments?sentiment=positivo
/api/comments?product_id=PROD001&sentiment=negativo
```

Los resultados se devuelven ordenados por fecha de creación (descendente).

---

### 3️⃣ Resumen de sentimientos

**GET** `/api/sentiment-summary`

#### Response
```json
{
  "total_comments": 100,
  "sentiment_counts": {
    "positivo": 60,
    "negativo": 20,
    "neutral": 20
  }
}
```

---

## 🧠 Análisis de Sentimiento

La integración con Inteligencia Artificial se realiza **directamente en el backend**, dentro de la capa de aplicación.

### 📍 Ubicación del código de IA

La lógica que consume la API de **Google Gemini** se encuentra implementada en el siguiente archivo:

```
Application/Services/GeminiSentimentAnalyzer.cs
```

Este archivo contiene la implementación concreta del análisis de sentimiento utilizando una llamada HTTP a la API de Gemini, construyendo un *prompt* específico para clasificar el texto como `positivo`, `negativo` o `neutral`.

La clase implementa la interfaz:

```
Application/Interfaces/ISentimentAnalyzer.cs
```

lo que permite desacoplar la lógica del controlador y facilita el uso de un **mecanismo de respaldo (fallback)**.

---

### 🔄 Flujo de análisis de sentimiento

El flujo de análisis de sentimiento es el siguiente:

1. Al recibir un comentario en el endpoint `POST /api/comments`, el controlador utiliza la interfaz `ISentimentAnalyzer`.
2. La implementación principal es `GeminiSentimentAnalyzer`, que:
   - Construye un prompt en lenguaje natural
   - Realiza una llamada HTTP a la API de Google Gemini
   - Interpreta la respuesta y normaliza el resultado

3. Si ocurre cualquier error durante la llamada a Gemini:
   - Error de red
   - API Key inválida
   - Error HTTP

   se utiliza automáticamente la implementación de respaldo:

```
Application/Services/RuleBasedSentimentAnalyzer.cs
```

la cual clasifica el sentimiento usando reglas simples basadas en palabras clave, tal como se especifica en el documento de la prueba técnica.

### 🧩 Palabras clave utilizadas

- **Positivo:** `excelente`, `genial`, `fantástico`, `bueno`, `increíble`
- **Negativo:** `malo`, `terrible`, `problema`, `defecto`, `horrible`
- **Otro caso:** `neutral`

---

## 🧪 Testing

El proyecto incluye pruebas automatizadas básicas para validar tanto la lógica del análisis de sentimientos como el comportamiento de la API:

- **Pruebas unitarias:** cubren el analizador de sentimientos basado en reglas para garantizar una clasificación correcta según las palabras clave.
- **Pruebas de integración:** verifican el flujo completo de POST y GET con la base de datos mediante un proveedor en memoria.

Las pruebas se pueden ejecutar con:
```bash
dotnet test
```

---

## 🛠️ Decisiones de diseño

- Uso de **interfaces** para desacoplar el análisis de sentimiento
- Mecanismo de fallback automático sin intervención del cliente
- Arquitectura en capas para facilitar mantenimiento y escalabilidad
- DTOs para evitar exponer entidades directamente
- Docker Compose para simplificar la ejecución del entorno completo

---

## 📌 Notas finales

- El proyecto está pensado para fines demostrativos y evaluación técnica.
- En un entorno productivo se recomienda:
  - Manejo de errores más robusto
  - Logging estructurado
  - Tests automatizados
  - Secrets management

---

## 👤 Autor

**Luis Fernando Félix Mata**  
Backend Developer

---

