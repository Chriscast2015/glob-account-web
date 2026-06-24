import { useEffect, useRef, useState } from "react";
import {
  FaClock,
  FaEnvelope,
  FaMapMarkerAlt,
  FaPaperPlane,
  FaPhoneAlt,
} from "react-icons/fa";

const initialValues = {
  name: "",
  email: "",
  phone: "",
  message: "",
  website: "",
};

const fieldLimits = {
  name: 100,
  email: 254,
  phone: 30,
  message: 2000,
  website: 200,
};

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const phonePattern = /^[0-9+()\-\s.]*$/;
const turnstileSiteKey = (import.meta.env.VITE_TURNSTILE_SITE_KEY ?? "").trim();
const isCaptchaEnabled = Boolean(turnstileSiteKey);
const turnstileScriptId = "cloudflare-turnstile-script";
const mapEmbedUrl =
  "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3989.7959885320383!2d-78.4857236!3d-0.19170049999999994!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x91d59b279cddf2c5%3A0x52cb63ce525fc5e9!2sGlobal%20Account%20Services!5e0!3m2!1ses-419!2sec!4v1782331409684!5m2!1ses-419!2sec";

const contactDetails = [
  {
    id: "address",
    icon: FaMapMarkerAlt,
    label: "Av. República E6-447 y Av. Eloy Alfaro, Quito - Ecuador",
  },
  {
    id: "phone",
    icon: FaPhoneAlt,
    label: "+593 98 461 9838",
  },
  {
    id: "email",
    icon: FaEnvelope,
    label: "hrcastros@yahoo.com",
  },
  {
    id: "hours",
    icon: FaClock,
    label: "Lunes a Viernes 09:00 - 17:00",
  },
];

function getContactEndpoint() {
  const apiBaseUrl = (import.meta.env.VITE_API_URL ?? "").replace(/\/$/, "");
  return `${apiBaseUrl}/api/contact`;
}

function validateContactForm(values) {
  const nextErrors = {};
  const trimmedName = values.name.trim();
  const trimmedEmail = values.email.trim();
  const trimmedPhone = values.phone.trim();
  const trimmedMessage = values.message.trim();

  if (!trimmedName) {
    nextErrors.name = "Ingresa tu nombre.";
  } else if (trimmedName.length > fieldLimits.name) {
    nextErrors.name = `El nombre no puede superar ${fieldLimits.name} caracteres.`;
  }

  if (!trimmedEmail) {
    nextErrors.email = "Ingresa tu email.";
  } else if (trimmedEmail.length > fieldLimits.email) {
    nextErrors.email = `El email no puede superar ${fieldLimits.email} caracteres.`;
  } else if (!emailPattern.test(trimmedEmail)) {
    nextErrors.email = "Ingresa un email válido.";
  }

  if (trimmedPhone.length > fieldLimits.phone) {
    nextErrors.phone = `El teléfono no puede superar ${fieldLimits.phone} caracteres.`;
  } else if (trimmedPhone && !phonePattern.test(trimmedPhone)) {
    nextErrors.phone = "El teléfono contiene caracteres no válidos.";
  }

  if (!trimmedMessage) {
    nextErrors.message = "Ingresa tu mensaje.";
  } else if (trimmedMessage.length < 10) {
    nextErrors.message = "El mensaje debe tener al menos 10 caracteres.";
  } else if (trimmedMessage.length > fieldLimits.message) {
    nextErrors.message = `El mensaje no puede superar ${fieldLimits.message} caracteres.`;
  }

  return nextErrors;
}

function normalizeServerErrors(serverErrors) {
  if (!serverErrors) {
    return {};
  }

  const fieldMap = {
    Name: "name",
    Email: "email",
    Phone: "phone",
    Message: "message",
    name: "name",
    email: "email",
    phone: "phone",
    message: "message",
  };

  return Object.entries(serverErrors).reduce((nextErrors, [field, messages]) => {
    const normalizedField = fieldMap[field];

    if (!normalizedField) {
      return nextErrors;
    }

    nextErrors[normalizedField] = Array.isArray(messages)
      ? messages[0]
      : String(messages);

    return nextErrors;
  }, {});
}

export default function Contact() {
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState({});
  const [status, setStatus] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [captchaToken, setCaptchaToken] = useState("");
  const [captchaError, setCaptchaError] = useState("");
  const [isTurnstileReady, setIsTurnstileReady] = useState(false);
  const [formStartedAt, setFormStartedAt] = useState(() => Date.now());
  const turnstileRef = useRef(null);
  const turnstileWidgetId = useRef(null);

  useEffect(() => {
    if (!isCaptchaEnabled) {
      return undefined;
    }

    const handleLoad = () => {
      setIsTurnstileReady(Boolean(window.turnstile));
    };

    const handleError = () => {
      setCaptchaError("No pudimos cargar la validación de seguridad.");
    };

    if (window.turnstile) {
      handleLoad();
      return undefined;
    }

    let script = document.getElementById(turnstileScriptId);

    if (!script) {
      script = document.createElement("script");
      script.id = turnstileScriptId;
      script.src = "https://challenges.cloudflare.com/turnstile/v0/api.js";
      script.async = true;
      script.defer = true;
      document.head.appendChild(script);
    }

    script.addEventListener("load", handleLoad);
    script.addEventListener("error", handleError);

    return () => {
      script.removeEventListener("load", handleLoad);
      script.removeEventListener("error", handleError);
    };
  }, []);

  useEffect(() => {
    if (
      !isCaptchaEnabled ||
      !isTurnstileReady ||
      !turnstileRef.current ||
      turnstileWidgetId.current ||
      !window.turnstile
    ) {
      return undefined;
    }

    turnstileWidgetId.current = window.turnstile.render(turnstileRef.current, {
      sitekey: turnstileSiteKey,
      callback: (token) => {
        setCaptchaToken(token);
        setCaptchaError("");
      },
      "expired-callback": () => {
        setCaptchaToken("");
      },
      "error-callback": () => {
        setCaptchaToken("");
        setCaptchaError("No pudimos validar el captcha. Inténtalo nuevamente.");
      },
    });

    return () => {
      if (window.turnstile && turnstileWidgetId.current) {
        window.turnstile.remove(turnstileWidgetId.current);
        turnstileWidgetId.current = null;
      }
    };
  }, [isTurnstileReady]);

  const resetTurnstile = () => {
    if (window.turnstile && turnstileWidgetId.current) {
      window.turnstile.reset(turnstileWidgetId.current);
    }

    setCaptchaToken("");
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    setValues((currentValues) => ({
      ...currentValues,
      [name]: value,
    }));

    if (errors[name]) {
      setErrors((currentErrors) => {
        const nextErrors = { ...currentErrors };
        delete nextErrors[name];
        return nextErrors;
      });
    }

    if (status) {
      setStatus(null);
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    const nextErrors = validateContactForm(values);

    if (Object.keys(nextErrors).length > 0) {
      setErrors(nextErrors);
      setStatus(null);
      return;
    }

    if (isCaptchaEnabled && !captchaToken) {
      setStatus({
        type: "error",
        message: "Completa la validación de seguridad antes de enviar.",
      });
      return;
    }

    const payload = {
      name: values.name.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      message: values.message.trim(),
      website: values.website.trim(),
      startedAt: formStartedAt,
      turnstileToken: captchaToken || null,
    };

    setIsSubmitting(true);
    setStatus(null);

    try {
      const response = await fetch(getContactEndpoint(), {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      const data = await response.json().catch(() => null);

      if (!response.ok) {
        if (response.status === 400) {
          const serverErrors = normalizeServerErrors(data?.errors);

          if (Object.keys(serverErrors).length > 0) {
            setErrors(serverErrors);
          }

          setStatus({
            type: "error",
            message: data?.message ?? "Revisa los datos del formulario.",
          });

          return;
        }

        if (response.status === 403) {
          setStatus({
            type: "error",
            message: data?.message ?? "No pudimos validar la solicitud.",
          });
          return;
        }

        if (response.status === 429) {
          setStatus({
            type: "error",
            message: "Recibimos muchos intentos. Espera unos minutos antes de volver a enviar.",
          });
          return;
        }

        throw new Error(data?.message ?? "Contact request failed");
      }

      setValues(initialValues);
      setErrors({});
      setFormStartedAt(Date.now());
      setStatus({
        type: "success",
        message: data?.message ?? "Mensaje enviado. Te contactaremos pronto.",
      });
    } catch {
      setStatus({
        type: "error",
        message: "No pudimos enviar el mensaje. Inténtalo nuevamente.",
      });
    } finally {
      if (isCaptchaEnabled) {
        resetTurnstile();
      }

      setIsSubmitting(false);
    }
  };

  return (
    <section id="contact" className="contact">
      <div className="container">
        <div className="contact-panel">
          <form onSubmit={handleSubmit} noValidate>
            <h2>Cont&aacute;ctanos</h2>
            <div className="contact-form-grid">
              <div className="form-row form-honeypot" aria-hidden="true">
                <label htmlFor="website">Sitio web</label>
                <input
                  id="website"
                  name="website"
                  type="text"
                  value={values.website}
                  onChange={handleChange}
                  maxLength={fieldLimits.website}
                  tabIndex="-1"
                  autoComplete="off"
                />
              </div>

              <div className="form-row">
                <label htmlFor="name">Nombre</label>
                <input
                  id="name"
                  name="name"
                  type="text"
                  placeholder="Nombre"
                  value={values.name}
                  onChange={handleChange}
                  className={errors.name ? "is-invalid" : ""}
                  aria-invalid={Boolean(errors.name)}
                  aria-describedby={errors.name ? "name-error" : undefined}
                  autoComplete="name"
                  maxLength={fieldLimits.name}
                />
                {errors.name && (
                  <p className="field-error" id="name-error">
                    {errors.name}
                  </p>
                )}
              </div>

              <div className="form-row">
                <label htmlFor="email">Email</label>
                <input
                  id="email"
                  name="email"
                  type="email"
                  placeholder="Email"
                  value={values.email}
                  onChange={handleChange}
                  className={errors.email ? "is-invalid" : ""}
                  aria-invalid={Boolean(errors.email)}
                  aria-describedby={errors.email ? "email-error" : undefined}
                  autoComplete="email"
                  maxLength={fieldLimits.email}
                />
                {errors.email && (
                  <p className="field-error" id="email-error">
                    {errors.email}
                  </p>
                )}
              </div>

              <div className="form-row">
                <label htmlFor="phone">Teléfono</label>
                <input
                  id="phone"
                  name="phone"
                  type="tel"
                  placeholder="Teléfono"
                  value={values.phone}
                  onChange={handleChange}
                  className={errors.phone ? "is-invalid" : ""}
                  aria-invalid={Boolean(errors.phone)}
                  aria-describedby={errors.phone ? "phone-error" : undefined}
                  autoComplete="tel"
                  maxLength={fieldLimits.phone}
                />
                {errors.phone && (
                  <p className="field-error" id="phone-error">
                    {errors.phone}
                  </p>
                )}
              </div>

              <div className="form-row form-row-message">
                <label htmlFor="message">Mensaje</label>
                <textarea
                  id="message"
                  name="message"
                  placeholder="Mensaje"
                  value={values.message}
                  onChange={handleChange}
                  className={errors.message ? "is-invalid" : ""}
                  aria-invalid={Boolean(errors.message)}
                  aria-describedby={errors.message ? "message-error" : undefined}
                  autoComplete="off"
                  maxLength={fieldLimits.message}
                ></textarea>
                {errors.message && (
                  <p className="field-error" id="message-error">
                    {errors.message}
                  </p>
                )}
              </div>
            </div>

            {isCaptchaEnabled && (
              <div className="captcha-row">
                <div ref={turnstileRef} />
                {captchaError && (
                  <p className="field-error" role="status">
                    {captchaError}
                  </p>
                )}
              </div>
            )}

            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Enviando..." : "Enviar"}
              <FaPaperPlane aria-hidden="true" />
            </button>
            {status && (
              <p className={`form-status is-${status.type}`} role="status" aria-live="polite">
                {status.message}
              </p>
            )}
          </form>

          <aside className="contact-info" aria-label="Información de contacto">
            <h3>Información de contacto</h3>
            <div className="contact-info-list">
              {contactDetails.map(({ id, icon: Icon, label }) => (
                <div className="contact-info-item" key={id}>
                  <span className="contact-info-icon" aria-hidden="true">
                    <Icon />
                  </span>
                  <p>{label}</p>
                </div>
              ))}
            </div>
          </aside>

          <div className="map">
            <iframe
              title="Ubicación Global Account Services"
              src={mapEmbedUrl}
              width="100%"
              height="300"
              style={{ border: 0 }}
              allowFullScreen
              loading="lazy"
              referrerPolicy="no-referrer"
              sandbox="allow-scripts allow-same-origin allow-popups allow-popups-to-escape-sandbox allow-forms"
            ></iframe>
          </div>
        </div>
      </div>
    </section>
  );
}
