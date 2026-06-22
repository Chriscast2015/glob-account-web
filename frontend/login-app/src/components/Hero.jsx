import { useEffect, useRef } from "react";
import { FaShieldAlt, FaUserCheck, FaUsers } from "react-icons/fa";
import heroLogo900 from "../assets/hero-logo-900.webp";
import heroLogo1350 from "../assets/hero-logo-1350.webp";
import { handleLandingSectionClick } from "../utils/landingNavigation";

const trustItems = [
  {
    id: "experience",
    label: <>+20 a&ntilde;os de experiencia</>,
    icon: FaUsers,
  },
  {
    id: "personalized",
    label: <>Asesor&iacute;a personalizada</>,
    icon: FaUserCheck,
  },
  {
    id: "compliance",
    label: <>Cumplimiento contable y tributario</>,
    icon: FaShieldAlt,
  },
];

export default function Hero() {
  const heroRef = useRef(null);
  const backgroundRef = useRef(null);

  useEffect(() => {
    let animationFrame = null;
    let layerReleaseTimer = null;
    const backgroundElement = backgroundRef.current;

    const updateBackgroundOpacity = () => {
      const hero = heroRef.current;
      const background = backgroundRef.current;

      if (!hero || !background) {
        return;
      }

      const fadeDistance = Math.max(hero.offsetHeight * 0.72, 520);
      const scrollWithinHero = Math.max(window.scrollY - hero.offsetTop, 0);
      const progress = Math.min(scrollWithinHero / fadeDistance, 1);

      background.style.opacity = String(1 - progress);
    };

    const handleScroll = () => {
      if (backgroundElement) {
        backgroundElement.style.willChange = "opacity";
      }

      if (layerReleaseTimer !== null) {
        window.clearTimeout(layerReleaseTimer);
      }

      layerReleaseTimer = window.setTimeout(() => {
        if (backgroundElement) {
          backgroundElement.style.removeProperty("will-change");
        }

        layerReleaseTimer = null;
      }, 180);

      if (animationFrame !== null) {
        window.cancelAnimationFrame(animationFrame);
      }

      animationFrame = window.requestAnimationFrame(() => {
        updateBackgroundOpacity();
        animationFrame = null;
      });
    };

    updateBackgroundOpacity();
    window.addEventListener("scroll", handleScroll, { passive: true });
    window.addEventListener("resize", handleScroll);

    return () => {
      if (animationFrame !== null) {
        window.cancelAnimationFrame(animationFrame);
      }

      if (layerReleaseTimer !== null) {
        window.clearTimeout(layerReleaseTimer);
      }

      if (backgroundElement) {
        backgroundElement.style.removeProperty("will-change");
      }

      window.removeEventListener("scroll", handleScroll);
      window.removeEventListener("resize", handleScroll);
    };
  }, []);

  return (
    <section id="hero" className="hero" ref={heroRef}>
      <div className="hero-background" ref={backgroundRef} aria-hidden="true" />
      <div className="container">
        <div className="hero-copy">
          <h1>
            Tu aliado en
            <br />
            gesti&oacute;n
            <br />
            financiera
          </h1>
          <p>
            Soluciones contables y de gesti&oacute;n para empresas modernas.
            <br />
            Acompa&ntilde;amos tu crecimiento con responsabilidad,
            <br />
            experiencia y soluciones a la medida.
          </p>
          <div className="hero-buttons">
            <a
              href="#contact"
              className="btn"
              onClick={(event) => handleLandingSectionClick(event, "contact")}
            >
              Solicita una asesoría <span className="btn-arrow" aria-hidden="true">&rarr;</span>
            </a>
            <a
              href="#services"
              className="btn btn-secondary"
              onClick={(event) => handleLandingSectionClick(event, "services")}
            >
              Nuestros servicios
            </a>
          </div>
          <div className="hero-trust" aria-label="Indicadores de confianza">
            {trustItems.map(({ id, label, icon: Icon }) => (
              <div className="trust-item" key={id}>
                <span className="trust-icon" aria-hidden="true">
                  <Icon />
                </span>
                <span>{label}</span>
              </div>
            ))}
          </div>
        </div>
        <div className="hero-art">
          <img
            src={heroLogo900}
            srcSet={`${heroLogo900} 900w, ${heroLogo1350} 1350w`}
            sizes="(max-width: 820px) 72vw, 422px"
            width="900"
            height="696"
            alt="Global Account Services"
            fetchPriority="high"
            decoding="async"
          />
        </div>
      </div>
    </section>
  );
}
