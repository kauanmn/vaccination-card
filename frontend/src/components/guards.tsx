import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function RequireAuth() {
  const { session } = useAuth();
  if (!session) return <Navigate to="/login" replace />;
  return <Outlet />;
}

export function RequireAdmin() {
  const { isAdmin } = useAuth();
  if (!isAdmin) return <Navigate to="/" replace />;
  return <Outlet />;
}

/** Rota inicial: admin vê a lista de pacientes; paciente vê o próprio cartão. */
export function HomeRedirect() {
  const { session, isAdmin } = useAuth();

  if (!session) return <Navigate to="/login" replace />;
  if (isAdmin) return <Navigate to="/patients" replace />;
  if (session.patientId) return <Navigate to={`/patients/${session.patientId}`} replace />;
  return <Navigate to="/vaccines" replace />;
}
