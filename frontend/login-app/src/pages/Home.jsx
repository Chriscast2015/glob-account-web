import Navbar from "../components/Navbar";
import Hero from "../components/Hero";
import About from "../components/About";
import Services from "../components/Services";
import Contact from "../components/Contact";
import Footer from "../components/Footer";
import WhatsAppButton from "../components/WhatsAppButton";

export default function Home() {
  return (
    <>
      <a className="skip-link" href="#main-content">
        Saltar al contenido
      </a>
      <Navbar />
      <main id="main-content" tabIndex="-1">
        <Hero />
        <About />
        <Services />
        <Contact />
      </main>
      <Footer />
      <WhatsAppButton />
    </>
  );
}
