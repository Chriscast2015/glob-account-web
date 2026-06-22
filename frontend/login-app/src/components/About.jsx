import {
  FaBalanceScale,
  FaCalculator,
  FaChartLine,
  FaFileInvoiceDollar,
  FaHandshake,
  FaHeadset,
  FaUserTie,
} from "react-icons/fa";
import aboutOffice768 from "../assets/about-office-768.webp";
import aboutOffice1200 from "../assets/about-office-1200.webp";
import aboutOffice1536 from "../assets/about-office-1536.webp";

const capabilities = [
  FaCalculator,
  FaFileInvoiceDollar,
  FaBalanceScale,
  FaChartLine,
];

const trustCards = [
  {
    id: "professionals",
    label: "Profesionales especializados",
    icon: FaUserTie,
  },
  {
    id: "attention",
    label: <>Atenci&oacute;n personalizada</>,
    icon: FaHeadset,
  },
  {
    id: "commitment",
    label: "Compromiso con nuestros clientes",
    icon: FaHandshake,
  },
];

export default function About() {
  return (
    <section id="about" className="about">
      <div className="container">
        <div className="about-media">
          <img
            src={aboutOffice768}
            srcSet={`${aboutOffice768} 768w, ${aboutOffice1200} 1200w, ${aboutOffice1536} 1536w`}
            sizes="(max-width: 820px) calc(100vw - 24px), 534px"
            width="1536"
            height="1024"
            alt="Oficina corporativa moderna con vista al parque La Carolina y la zona financiera de Quito"
            loading="lazy"
            decoding="async"
          />
          <div className="about-capabilities" aria-hidden="true">
            {capabilities.map((Icon, index) => (
              <span key={index}>
                <Icon />
              </span>
            ))}
          </div>
        </div>
        <div className="about-statement">
          <span className="about-accent" aria-hidden="true" />
          <p className="about-kicker">SOBRE NOSOTROS</p>
          <h2>Experiencia que genera confianza</h2>
          <p>
            Somos una firma de contadores independientes cuyos socios cuentan
            con amplia experiencia individual en diversas empresas a nivel
            nacional.
          </p>
          <p>
            Las pr&aacute;cticas y procedimientos que sigue la empresa est&aacute;n
            orientados al cumplimiento de los principios de contabilidad
            generalmente aceptados.
          </p>
          <div className="about-trust-cards" aria-label="Fortalezas de Global Account Services">
            {trustCards.map(({ id, label, icon: Icon }) => (
              <div className="about-trust-card" key={id}>
                <span className="about-trust-icon" aria-hidden="true">
                  <Icon />
                </span>
                <span>{label}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
