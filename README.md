# Glob Account Web

Landing publica de Glob Account con formulario de contacto seguro.

## Estructura

- `frontend/login-app`: landing en React + Vite.
- `backend/GlobAccountAPI`: API ASP.NET Core para el formulario de contacto.

## Configuracion

No se versionan secretos. Configura las variables en el proveedor de hosting.

Frontend:

```text
VITE_API_URL=https://tu-backend-render.onrender.com
VITE_TURNSTILE_SITE_KEY=tu_site_key_publica
```

Backend:

```text
ASPNETCORE_ENVIRONMENT=Production
AllowedHosts=localhost;127.0.0.1;globaccountservices.com;www.globaccountservices.com;api.globaccountservices.com;tu-backend.onrender.com
Jwt__Key=<clave-larga-configurada-en-render>
Smtp__Host=smtp.example.com
Smtp__Port=587
Smtp__Username=sender@example.com
Smtp__Password=<configurar-en-render>
Smtp__RecipientEmail=recipient@example.com
Smtp__EnableSsl=true
Smtp__TimeoutSeconds=10
Captcha__Enabled=true
Captcha__SecretKey=<configurar-en-render>
Cors__AllowedOrigins__0=https://globaccountservices.com
Cors__AllowedOrigins__1=https://www.globaccountservices.com
Cors__AllowedOrigins__2=https://tu-sitio-temporal.netlify.app
Cors__AllowedOrigins__3=http://localhost:5173
```

`ASPNETCORE_URLS` normalmente no hace falta en Render porque la API lee la
variable `PORT` que Render asigna. Si decides definirla manualmente, debe apuntar
al puerto asignado por Render, por ejemplo `http://0.0.0.0:<PORT>`.

Para staging puede usarse `AllowedHosts=*` solo temporalmente. Es comodo para
probar dominios aun no definitivos, pero desactiva el filtro de host y no debe
quedarse asi en produccion.

## Deploy backend en Render

Opcion nativa:

```text
Root Directory: backend/GlobAccountAPI
Build Command: dotnet publish -c Release -o out
Start Command: dotnet out/GlobAccountAPI.dll
```

Opcion Docker:

```text
Root Directory: backend/GlobAccountAPI
Dockerfile Path: Dockerfile
```

No guardes `Smtp__Password`, `Jwt__Key` ni `Captcha__SecretKey` en Git. Configuralos
solo en Render -> Environment Variables.

## Desarrollo

Frontend:

```powershell
cd frontend/login-app
npm install
npm run dev
```

Backend:

```powershell
cd backend/GlobAccountAPI
dotnet restore
dotnet run
```

## Verificacion

```powershell
dotnet build backend/GlobAccountAPI/GlobAccountAPI.sln --no-restore
npm run build --prefix frontend/login-app
npm run lint --prefix frontend/login-app
npm audit --prefix frontend/login-app --audit-level=moderate
dotnet list backend/GlobAccountAPI/GlobAccountAPI.csproj package --vulnerable --include-transitive
```
