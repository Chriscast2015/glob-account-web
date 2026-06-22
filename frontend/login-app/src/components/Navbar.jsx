import { useCallback, useEffect, useRef, useState } from "react";
import logo from "../assets/global-account-logo-360.webp";
import { handleLandingSectionClick } from "../utils/landingNavigation";

export default function Navbar() {
  const [isHidden, setIsHidden] = useState(false);
  const [isPreviewVisible, setIsPreviewVisible] = useState(false);
  const lastScrollY = useRef(0);
  const isHiddenRef = useRef(false);
  const previewTimer = useRef(null);
  const isNavigationPreviewActive = useRef(false);
  const ticking = useRef(false);

  const showNavbarPreview = useCallback((event) => {
    if (event?.detail?.sectionId === "hero") {
      isNavigationPreviewActive.current = false;
      isHiddenRef.current = false;
      setIsHidden(false);
      setIsPreviewVisible(false);

      if (previewTimer.current) {
        window.clearTimeout(previewTimer.current);
        previewTimer.current = null;
      }

      return;
    }

    isNavigationPreviewActive.current = true;
    isHiddenRef.current = true;
    setIsHidden(true);
    setIsPreviewVisible(true);

    if (previewTimer.current) {
      window.clearTimeout(previewTimer.current);
    }

    previewTimer.current = window.setTimeout(() => {
      setIsPreviewVisible(false);
      isNavigationPreviewActive.current = false;

      if (window.scrollY < 80) {
        isHiddenRef.current = false;
        setIsHidden(false);
      }
    }, 2600);
  }, []);

  useEffect(() => {
    lastScrollY.current = window.scrollY;

    const handleScroll = () => {
      if (ticking.current) {
        return;
      }

      ticking.current = true;

      window.requestAnimationFrame(() => {
        const currentScrollY = window.scrollY;
        const scrollDelta = currentScrollY - lastScrollY.current;

        if (currentScrollY < 80) {
          isHiddenRef.current = false;
          setIsHidden(false);
          setIsPreviewVisible(false);
          isNavigationPreviewActive.current = false;
        } else if (isNavigationPreviewActive.current) {
          lastScrollY.current = currentScrollY;
          ticking.current = false;
          return;
        } else if (scrollDelta > 8) {
          isHiddenRef.current = true;
          setIsHidden(true);
        } else if (scrollDelta < -8) {
          isHiddenRef.current = false;
          setIsHidden(false);
          setIsPreviewVisible(false);
        }

        lastScrollY.current = currentScrollY;
        ticking.current = false;
      });
    };

    const handleMouseMove = (event) => {
      if (!isHiddenRef.current || window.scrollY < 80) {
        return;
      }

      if (event.clientY <= 96) {
        showNavbarPreview();
      }
    };

    window.addEventListener("scroll", handleScroll, { passive: true });
    window.addEventListener("mousemove", handleMouseMove, { passive: true });
    window.addEventListener("navbar:navigation-preview", showNavbarPreview);

    return () => {
      if (previewTimer.current) {
        window.clearTimeout(previewTimer.current);
      }

      window.removeEventListener("scroll", handleScroll);
      window.removeEventListener("mousemove", handleMouseMove);
      window.removeEventListener("navbar:navigation-preview", showNavbarPreview);
    };
  }, [showNavbarPreview]);

  return (
    <header
      className={`navbar${isHidden ? " navbar-hidden" : ""}${
        isPreviewVisible ? " navbar-preview" : ""
      }`}
    >
      <div className="container">
        <a href="/" className="logo" aria-label="Global Account Services">
          <img
            src={logo}
            width="360"
            height="144"
            alt="Global Account Services"
            decoding="async"
          />
        </a>
        <nav>
          <ul className="nav-list">
            <li>
              <a
                href="#hero"
                onClick={(event) =>
                  handleLandingSectionClick(event, "hero", { block: "start" })
                }
              >
                Inicio
              </a>
            </li>
            <li>
              <a
                href="#about"
                onClick={(event) => handleLandingSectionClick(event, "about")}
              >
                Nosotros
              </a>
            </li>
            <li>
              <a
                href="#services"
                onClick={(event) =>
                  handleLandingSectionClick(event, "services")
                }
              >
                Servicios
              </a>
            </li>
            <li>
              <a
                href="#contact"
                onClick={(event) => handleLandingSectionClick(event, "contact")}
              >
                Contacto
              </a>
            </li>
          </ul>
        </nav>
      </div>
    </header>
  );
}
