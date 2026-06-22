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
Smtp__Host=smtp.example.com
Smtp__Port=587
Smtp__Username=sender@example.com
Smtp__Password=secret
Smtp__RecipientEmail=recipient@example.com
Captcha__Enabled=true
Captcha__SecretKey=secret
Cors__AllowedOrigins__0=https://globaccountservices.com
Cors__AllowedOrigins__1=https://www.globaccountservices.com
```

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
