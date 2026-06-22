const MOBILE_NAVIGATION_QUERY = "(max-width: 820px)";
const MOBILE_SECTION_OFFSET = 12;

export function scrollToLandingSection(sectionId, options = {}) {
  const { block = "center", previewNavbar = true } = options;
  const section = document.getElementById(sectionId);

  if (!section) {
    return false;
  }

  const isMobile = window.matchMedia(MOBILE_NAVIGATION_QUERY).matches;

  if (isMobile) {
    const navbarHeight =
      document.querySelector(".navbar")?.getBoundingClientRect().height ?? 0;
    const sectionTop =
      section.getBoundingClientRect().top +
      window.scrollY -
      navbarHeight -
      MOBILE_SECTION_OFFSET;

    window.scrollTo({
      top: Math.max(0, sectionTop),
      behavior: "smooth",
    });
  } else {
    section.scrollIntoView({
      behavior: "smooth",
      block,
      inline: "center",
    });
  }

  if (previewNavbar) {
    window.dispatchEvent(
      new CustomEvent("navbar:navigation-preview", {
        detail: { sectionId },
      }),
    );
  }

  window.history.pushState(null, "", `#${sectionId}`);
  return true;
}

export function handleLandingSectionClick(event, sectionId, options) {
  event.preventDefault();

  if (!scrollToLandingSection(sectionId, options)) {
    window.location.href = `/#${sectionId}`;
  }
}
