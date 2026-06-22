# Global Account Services Frontend

Frontend en React + Vite para la landing page y el formulario de contacto de Global Account Services.

## Ubicacion del proyecto

La app real vive en:

```bash
frontend/login-app
```

El `package.json` de `frontend` funciona como wrapper, por lo que tambien puedes ejecutar los scripts desde `frontend`.

## Scripts

Desde `frontend`:

```bash
npm run dev
npm run build
npm run lint
npm run preview
```

Desde `frontend/login-app`:

```bash
npm run dev
npm run build
npm run lint
npm run preview
```

## Desarrollo local

El servidor de Vite usa el puerto `5173`.

```bash
npm run dev
```

La ruta `/api` se proxya hacia el backend configurado en `vite.config.js` para integraciones como el formulario de contacto:

```js
target: "http://localhost:5093"
```

Si `VITE_API_URL` no esta definido en desarrollo, el formulario llama a
`/api/contact` y Vite usa el proxy local anterior. Para un deploy estatico,
define `VITE_API_URL` con la URL publica HTTPS del backend.

## Deploy en Render Static Site

Configura el servicio asi:

```text
Root Directory: frontend/login-app
Build Command: npm install && npm run build
Publish Directory: dist
```

Variables de entorno:

```text
VITE_API_URL=https://tu-backend-en-render.onrender.com
VITE_TURNSTILE_SITE_KEY=tu_site_key_publica
```

`VITE_API_URL` debe ser la URL HTTPS real del Render Web Service del backend,
sin `/api/contact` al final. Ejemplo: `https://nombre-api.onrender.com`.

### Headers de seguridad

El archivo `public/_headers` conserva una CSP segura para entornos que lean ese
formato, pero Render Static Site documenta los headers personalizados desde el
Dashboard, no desde un archivo `_headers`. En Render, agrega estos headers con
Path `/*` en la seccion de Custom Headers:

```text
X-Content-Type-Options: nosniff
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: camera=(), microphone=(), geolocation=()
X-Frame-Options: DENY
Content-Security-Policy: default-src 'self'; script-src 'self' https://challenges.cloudflare.com; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; img-src 'self' data: https://www.google.com https://maps.gstatic.com https://*.googleusercontent.com; font-src 'self' data: https://fonts.gstatic.com; connect-src 'self' https://globaccountservices.com https://www.globaccountservices.com https://*.onrender.com https://challenges.cloudflare.com; frame-src https://www.google.com https://challenges.cloudflare.com; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'; upgrade-insecure-requests
```

## Rutas

- `/`: landing publica.

Las rutas antiguas o no disponibles redirigen a la landing.
