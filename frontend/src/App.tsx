import { Route, Routes } from "react-router-dom";
import { Layout } from "./components/Layout";
import { HomeRedirect, RequireAdmin, RequireAuth } from "./components/guards";
import { CardPage } from "./pages/CardPage";
import { LoginPage } from "./pages/LoginPage";
import { NotFoundPage } from "./pages/NotFoundPage";
import { PatientsPage } from "./pages/PatientsPage";
import { VaccinesPage } from "./pages/VaccinesPage";

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<HomeRedirect />} />

      <Route element={<RequireAuth />}>
        <Route element={<Layout />}>
          <Route path="/vaccines" element={<VaccinesPage />} />
          <Route path="/patients/:id" element={<CardPage />} />

          <Route element={<RequireAdmin />}>
            <Route path="/patients" element={<PatientsPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
