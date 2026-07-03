import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-4 text-center">
      <div className="text-5xl" aria-hidden>
        🔍
      </div>
      <h1 className="text-2xl font-bold text-slate-900">Página não encontrada</h1>
      <Link to="/" className="btn-primary">
        Voltar ao início
      </Link>
    </div>
  );
}
