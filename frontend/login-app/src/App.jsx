import { useEffect } from "react";
import Home from "./pages/Home";

function App() {
  useEffect(() => {
    if (window.location.pathname !== "/") {
      window.history.replaceState(null, "", "/");
    }
  }, []);

  return <Home />;
}

export default App;
