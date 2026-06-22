import {
  FaEnvelope,
  FaMapMarkerAlt,
  FaPhoneAlt,
} from "react-icons/fa";
import antLogo from "../assets/ant-logo-128.webp";
import companyLogo from "../assets/global-account-logo-360.webp";
import iessLogo from "../assets/iess-logo-128.webp";
import sriLogo from "../assets/sri-logo-128.webp";
import superciasLogo from "../assets/supercias-logo-128.webp";
import { handleLandingSectionClick } from "../utils/landingNavigation";

const quickLinks = [
  { label: "Inicio", href: "#hero" },
  { label: "Nosotros", href: "#about" },
  { label: "Servicios", href: "#services" },
  { label: "Contacto", href: "#contact" },
];

const serviceLinks = [
  "BPO",
  "Asesoría Tributaria",
  "Asesoría Laboral",
  "Asesoría Societaria",
  "Consultoría Empresarial",
  "InventSoft ERP",
];

const socialLinks = [
  {
    label: "SRI",
    href: "https://www.sri.gob.ec/web/intersri/home",
    image: sriLogo,
  },
  {
    label: "Supercias",
    href: "https://www.supercias.gob.ec/portalscvs/index.htm",
    image: superciasLogo,
  },
  {
    label: "IESS",
    href: "https://www.iess.gob.ec/",
    image: iessLogo,
  },
  {
    label: "ANT",
    href: "https://www.ant.gob.ec/",
    image: antLogo,
  },
];

export default function Footer() {
  return (
    <footer className="footer">
      <div className="container">
        <div className="footer-grid">
          <div className="footer-brand">
            <a
              className="footer-logo"
              href="#hero"
              aria-label="Ir al inicio"
              onClick={(event) =>
                handleLandingSectionClick(event, "hero", { block: "start" })
              }
            >
              <img
                src={companyLogo}
                width="360"
                height="144"
                alt="Global Account Services"
                loading="lazy"
                decoding="async"
              />
            </a>
            <p>
              Soluciones contables y de gestión para empresas modernas.
            </p>
            <nav className="socials" aria-label="Enlaces externos">
              {socialLinks.map(({ label, href, icon: Icon, image }) => (
                <a
                  key={label}
                  href={href}
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label={`Abrir ${label}`}
                  title={label}
                >
                  <span className="social-icon" aria-hidden="true">
                    {Icon && <Icon />}
                    {image && (
                      <img
                        className="social-logo"
                        src={image}
                        width="128"
                        height="128"
                        alt=""
                        loading="lazy"
                        decoding="async"
                      />
                    )}
                  </span>
                </a>
              ))}
            </nav>
          </div>

          <nav className="footer-column" aria-label="Enlaces rápidos">
            <h2>Enlaces rápidos</h2>
            <ul>
              {quickLinks.map(({ label, href }) => (
                <li key={label}>
                  <a
                    href={href}
                    onClick={(event) =>
                      handleLandingSectionClick(event, href.replace("#", ""), {
                        block: href === "#hero" ? "start" : "center",
                      })
                    }
                  >
                    {label}
                  </a>
                </li>
              ))}
            </ul>
          </nav>

          <nav className="footer-column" aria-label="Nuestros servicios">
            <h2>Nuestros servicios</h2>
            <ul>
              {serviceLinks.map((service) => (
                <li key={service}>
                  <a
                    href="#services"
                    onClick={(event) =>
                      handleLandingSectionClick(event, "services")
                    }
                  >
                    {service}
                  </a>
                </li>
              ))}
            </ul>
          </nav>

          <div className="footer-column footer-contact">
            <h2>Contáctanos</h2>
            <ul>
              <li>
                <FaMapMarkerAlt aria-hidden="true" />
                <span>
                  Av. República E6-447 y Av. Eloy Alfaro, Quito - Ecuador
                </span>
              </li>
              <li>
                <FaPhoneAlt aria-hidden="true" />
                <span>+593 98 461 9838</span>
              </li>
              <li>
                <FaEnvelope aria-hidden="true" />
                <span>hrcastros@yahoo.com</span>
              </li>
            </ul>
          </div>
        </div>

        <div className="footer-bottom">
          <p>
            &copy; 2026 Global Account Services. Todos los derechos reservados.
          </p>
        </div>
      </div>
    </footer>
  );
}
