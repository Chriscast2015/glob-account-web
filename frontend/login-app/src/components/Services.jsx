import { useEffect, useState } from "react";
import {
  FaBuilding,
  FaCalculator,
  FaChartLine,
  FaDesktop,
  FaLaptopCode,
  FaUsers,
} from "react-icons/fa";

const MOBILE_CARD_CLOSE_DELAY = 3500;
const mobileInteractionQuery =
  "(max-width: 768px), (hover: none), (pointer: coarse)";

const services = [
  {
    id: "bpo",
    title: "BPO",
    subtitle: "Business Process Outsourcing / Outsourcing Contable",
    icon: FaDesktop,
    description:
      "Gestionamos tus procesos contables y administrativos para que tu empresa optimice tiempo, reduzca errores y se enfoque en su crecimiento estratégico.",
  },
  {
    id: "tributaria",
    title: "Asesoría Tributaria",
    subtitle: "Gestión y cumplimiento tributario",
    icon: FaCalculator,
    description:
      "Brindamos acompañamiento en el cumplimiento de tus obligaciones tributarias, ayudándote a optimizar procesos y minimizar riesgos fiscales.",
  },
  {
    id: "laboral",
    title: "Asesoría Laboral",
    subtitle: "Gestión de nómina y asesoría laboral",
    icon: FaUsers,
    description:
      "Te asesoramos en la correcta administración laboral de tu empresa, incluyendo nómina, contratos y cumplimiento de obligaciones legales con tus colaboradores.",
  },
  {
    id: "societaria",
    title: "Asesoría Societaria",
    subtitle: "Gestionamos y constituimos tu compañía",
    icon: FaBuilding,
    description:
      "Te acompañamos en la constitución, actualización y gestión legal de tu empresa, asegurando el cumplimiento de los requisitos societarios vigentes.",
  },
  {
    id: "consultoria",
    title: "Consultoría Empresarial",
    subtitle: "Soluciones para la gestión de tu negocio",
    icon: FaChartLine,
    description:
      "Analizamos tu empresa y desarrollamos estrategias prácticas para mejorar procesos, fortalecer la organización e impulsar la rentabilidad y el crecimiento.",
  },
  {
    id: "inventsoft",
    title: "InventSoft ERP",
    subtitle: "Software contable y financiero",
    icon: FaLaptopCode,
    description:
      "Impulsamos la gestión de tu empresa con herramientas digitales que permiten organizar información contable, financiera y administrativa de forma más eficiente.",
    href: "https://inventsoft-ec.com/",
  },
];

export default function Services() {
  const [activeService, setActiveService] = useState(null);

  useEffect(() => {
    if (
      !activeService ||
      !window.matchMedia(mobileInteractionQuery).matches
    ) {
      return undefined;
    }

    const closeTimer = window.setTimeout(() => {
      setActiveService(null);
    }, MOBILE_CARD_CLOSE_DELAY);

    return () => {
      window.clearTimeout(closeTimer);
    };
  }, [activeService]);

  const toggleService = (event, serviceTitle) => {
    if (event.target.closest("a")) {
      return;
    }

    const usesTouchInteraction = window.matchMedia(
      mobileInteractionQuery,
    ).matches;

    if (!usesTouchInteraction) {
      return;
    }

    setActiveService((currentService) =>
      currentService === serviceTitle ? null : serviceTitle,
    );
  };

  const handleServiceKeyDown = (event, serviceTitle) => {
    if (event.key !== "Enter" && event.key !== " ") {
      return;
    }

    event.preventDefault();
    setActiveService((currentService) =>
      currentService === serviceTitle ? null : serviceTitle,
    );
  };

  return (
    <section id="services" className="services">
      <div className="container">
        <div className="services-header">
          <h2>Servicios</h2>
          <p>
            Apoyo contable, tributario y empresarial para mantener tu operación
            clara, ordenada y lista para crecer.
          </p>
        </div>

        <div className="cards" aria-label="Servicios principales">
          {services.map((service) => {
            const ServiceIcon = service.icon;
            const detailsId = `service-${service.id}-details`;
            const hintId = `service-${service.id}-hint`;

            return (
              <article
                key={service.title}
                className={`service-card${
                  activeService === service.title ? " is-flipped" : ""
                }`}
                tabIndex="0"
                aria-expanded={activeService === service.title}
                aria-controls={detailsId}
                aria-describedby={hintId}
                aria-label={`${service.title}: ${service.subtitle}. ${service.description}`}
                onClick={(event) => toggleService(event, service.title)}
                onKeyDown={(event) =>
                  handleServiceKeyDown(event, service.title)
                }
              >
                <div className="service-card-inner">
                  <div className="service-card-face service-card-front">
                    <span className="service-icon" aria-hidden="true">
                      <ServiceIcon />
                    </span>
                    <h3>{service.title}</h3>
                    <p>{service.subtitle}</p>
                    <span className="sr-only" id={hintId}>
                      Toca o presiona Enter para ver la descripción del servicio.
                    </span>
                  </div>
                  <div
                    className="service-card-face service-card-back"
                    id={detailsId}
                  >
                    <p>{service.description}</p>
                    {service.href && (
                      <a
                        className="service-card-link"
                        href={service.href}
                        target="_blank"
                        rel="noopener noreferrer"
                        aria-label={`Ver más sobre ${service.title}`}
                      >
                        Ver más <span aria-hidden="true">→</span>
                      </a>
                    )}
                  </div>
                </div>
              </article>
            );
          })}
        </div>
      </div>
    </section>
  );
}
