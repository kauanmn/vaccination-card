import { useCallback, useEffect, useState } from "react";
import type { FormEvent } from "react";
import { createVaccine, listVaccines } from "../api/vaccines";
import { useAuth } from "../auth/AuthContext";
import { Modal } from "../components/Modal";
import { Pagination } from "../components/Pagination";
import { useToast } from "../components/Toast";
import { ApiError } from "../lib/api";
import type { Paged, Vaccine } from "../lib/types";

export function VaccinesPage() {
  const { isAdmin } = useAuth();
  const { toast } = useToast();

  const [page, setPage] = useState(1);
  const [data, setData] = useState<Paged<Vaccine> | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setLoadError(null);
    try {
      setData(await listVaccines(page, 20));
    } catch (err) {
      setLoadError(err instanceof ApiError ? err.message : "Erro ao carregar vacinas.");
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Vacinas</h1>
          <p className="mt-1 text-sm text-slate-500">Catálogo de vacinas disponíveis no sistema</p>
        </div>
        {isAdmin && (
          <button type="button" className="btn-primary" onClick={() => setCreating(true)}>
            + Nova vacina
          </button>
        )}
      </div>

      {loadError && <div className="alert-error mb-4">{loadError}</div>}

      <div className="card overflow-hidden">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-slate-200 bg-slate-50 text-xs font-semibold text-slate-500 uppercase">
            <tr>
              <th className="px-4 py-3">Nome</th>
              <th className="px-4 py-3">Esquema de doses</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {loading && (
              <tr>
                <td colSpan={2} className="px-4 py-8 text-center text-slate-400">
                  Carregando…
                </td>
              </tr>
            )}
            {!loading && data?.items.length === 0 && (
              <tr>
                <td colSpan={2} className="px-4 py-8 text-center text-slate-400">
                  Nenhuma vacina cadastrada.
                </td>
              </tr>
            )}
            {!loading &&
              data?.items.map((vaccine) => (
                <tr key={vaccine.id} className="hover:bg-slate-50">
                  <td className="px-4 py-3 font-medium text-slate-800">{vaccine.name}</td>
                  <td className="px-4 py-3">
                    {vaccine.totalDoses == null ? (
                      <span className="rounded-full bg-sky-100 px-2.5 py-0.5 text-xs font-semibold text-sky-700">
                        Periódica
                      </span>
                    ) : (
                      <span className="text-slate-600">
                        {vaccine.totalDoses} {vaccine.totalDoses === 1 ? "dose" : "doses"}
                      </span>
                    )}
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
        {data && <Pagination page={data.page} totalPages={data.totalPages} onChange={setPage} />}
      </div>

      {creating && (
        <CreateVaccineModal
          onClose={() => setCreating(false)}
          onCreated={(vaccine) => {
            setCreating(false);
            toast("success", `Vacina "${vaccine.name}" cadastrada.`);
            void load();
          }}
        />
      )}
    </div>
  );
}

function CreateVaccineModal({
  onClose,
  onCreated,
}: {
  onClose: () => void;
  onCreated: (vaccine: Vaccine) => void;
}) {
  const [name, setName] = useState("");
  const [periodic, setPeriodic] = useState(false);
  const [totalDoses, setTotalDoses] = useState("1");
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<{ name?: string; totalDoses?: string }>({});
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setFieldErrors({});

    try {
      const doses = periodic ? null : Number(totalDoses);
      const vaccine = await createVaccine(name.trim(), doses);
      onCreated(vaccine);
    } catch (err) {
      if (err instanceof ApiError) {
        setFieldErrors({
          name: err.fieldError("name"),
          totalDoses: err.fieldError("totalDoses"),
        });
        setError(err.details.length > 0 ? null : err.message);
      } else {
        setError("Erro inesperado. Tente novamente.");
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <Modal title="Nova vacina" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <div className="alert-error">{error}</div>}

        <div>
          <label htmlFor="vaccine-name" className="label">
            Nome
          </label>
          <input
            id="vaccine-name"
            className="input"
            value={name}
            onChange={(event) => setName(event.target.value)}
            placeholder="ex.: Hepatite B"
            autoFocus
          />
          {fieldErrors.name && <p className="field-error">{fieldErrors.name}</p>}
        </div>

        <label className="flex items-center gap-2 text-sm text-slate-700">
          <input
            type="checkbox"
            checked={periodic}
            onChange={(event) => setPeriodic(event.target.checked)}
            className="size-4 accent-teal-600"
          />
          Periódica (sem limite de doses, ex.: Gripe)
        </label>

        {!periodic && (
          <div>
            <label htmlFor="vaccine-doses" className="label">
              Total de doses
            </label>
            <input
              id="vaccine-doses"
              type="number"
              min={1}
              className="input"
              value={totalDoses}
              onChange={(event) => setTotalDoses(event.target.value)}
            />
            {fieldErrors.totalDoses && <p className="field-error">{fieldErrors.totalDoses}</p>}
          </div>
        )}

        <div className="flex justify-end gap-2 pt-2">
          <button type="button" className="btn-secondary" onClick={onClose} disabled={busy}>
            Cancelar
          </button>
          <button type="submit" className="btn-primary" disabled={busy}>
            {busy ? "Salvando…" : "Cadastrar"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
