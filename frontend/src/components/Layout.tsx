import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function Layout() {
  const { session, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
      isActive ? "bg-teal-600/15 text-teal-800" : "text-slate-600 hover:bg-slate-200/70"
    }`;

  return (
    <div className="min-h-screen">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-3">
          <div className="flex items-center gap-6">
            <span className="flex items-center gap-2 text-base font-bold text-slate-900">
              <span aria-hidden>💉</span> Cartão de Vacinação
            </span>
            <nav className="flex items-center gap-1">
              {isAdmin && (
                <NavLink to="/patients" end className={navLinkClass}>
                  Pacientes
                </NavLink>
              )}
              {!isAdmin && session?.patientId && (
                <NavLink to={`/patients/${session.patientId}`} className={navLinkClass}>
                  Meu cartão
                </NavLink>
              )}
              <NavLink to="/vaccines" className={navLinkClass}>
                Vacinas
              </NavLink>
            </nav>
          </div>

          <div className="flex items-center gap-3">
            <div className="text-right">
              <div className="text-sm font-semibold text-slate-800">{session?.name}</div>
              <div className="text-xs text-slate-500">{isAdmin ? "Administrador" : "Paciente"}</div>
            </div>
            <button type="button" className="btn-secondary" onClick={handleLogout}>
              Sair
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
