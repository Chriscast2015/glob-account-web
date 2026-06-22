import { FaWhatsapp } from "react-icons/fa";

export default function WhatsAppButton() {
  const phone = "+593984619838";
  const message = encodeURIComponent(
    "Hola, quiero más información sobre Global Account Services"
  );

  return (
    <a
      className="whatsapp-btn"
      href={`https://wa.me/${phone.replace(/\D/g, "")}?text=${message}`}
      target="_blank"
      rel="noopener noreferrer"
      title="WhatsApp"
    >
      <FaWhatsapp size={31} aria-hidden="true" />
      <span className="sr-only">Contactar por WhatsApp</span>
    </a>
  );
}
