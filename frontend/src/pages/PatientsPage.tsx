import { useCallback, useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link } from "react-router-dom";
import {
  createPatient,
  deletePatient,
  listPatients,
  updatePatient,
} from "../api/patients";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { Modal } from "../components/Modal";
import { Pagination } from "../components/Pagination";
import { useToast } from "../components/Toast";
import { ApiError } from "../lib/api";
import type { CreatedPatient, Paged, Patient } from "../lib/types";

export function PatientsPage() {
  const { toast } = useToast();

  const [page, setPage] = useState(1);
  const [data, setData] = useState<Paged<Patient> | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);
  const [editing, setEditing] = useState<Patient | null>(null);
  const [deleting, setDeleting] = useState<Patient | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setLoadError(null);
    try {
      const result = await listPatients(page, 20);

      if (result.items.length === 0 && page > 1) {
        setPage(page - 1);
        return;
      }
      setData(result);
    } catch (err) {
      setLoadError(
        err instanceof ApiError ? err.message : "Erro ao carregar pacientes.",
      );
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    void load();
  }, [load]);

  const handleDelete = async () => {
    if (!deleting) return;
    try {
      await deletePatient(deleting.id);
      toast("success", `Paciente "${deleting.name}" removido.`);
      setDeleting(null);
      void load();
    } catch (err) {
      setDeleting(null);
      toast(
        "error",
        err instanceof ApiError ? err.message : "Erro ao remover paciente.",
      );
    }
  };

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Pacientes</h1>
          <p className="mt-1 text-sm text-slate-500">
            Gerencie os pacientes e seus cartões de vacinação
          </p>
        </div>
        <button
          type="button"
          className="btn-primary"
          onClick={() => setCreating(true)}
        >
          + Novo paciente
        </button>
      </div>

      {loadError && <div className="alert-error mb-4">{loadError}</div>}

      <div className="card overflow-hidden">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-slate-200 bg-slate-50 text-xs font-semibold text-slate-500 uppercase">
            <tr>
              <th className="px-4 py-3">Nome</th>
              <th className="px-4 py-3">Vacinações registradas</th>
              <th className="px-4 py-3 text-right">Ações</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {loading && (
              <tr>
                <td
                  colSpan={3}
                  className="px-4 py-8 text-center text-slate-400"
                >
                  Carregando…
                </td>
              </tr>
            )}
            {!loading && data?.items.length === 0 && (
              <tr>
                <td
                  colSpan={3}
                  className="px-4 py-8 text-center text-slate-400"
                >
                  Nenhum paciente cadastrado. Clique em “Novo paciente” para
                  começar.
                </td>
              </tr>
            )}
            {!loading &&
              data?.items.map((patient) => (
                <tr key={patient.id} className="hover:bg-slate-50">
                  <td className="px-4 py-3 font-medium text-slate-800">
                    {patient.name}
                  </td>
                  <td className="px-4 py-3 text-slate-600">
                    {patient.vaccinations.length}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end gap-2">
                      <Link
                        to={`/patients/${patient.id}`}
                        className="btn-secondary"
                      >
                        Ver cartão
                      </Link>
                      <button
                        type="button"
                        className="btn-secondary"
                        onClick={() => setEditing(patient)}
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        className="btn-ghost text-red-600 hover:bg-red-50"
                        onClick={() => setDeleting(patient)}
                      >
                        Excluir
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
        {data && (
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            onChange={setPage}
          />
        )}
      </div>

      {creating && (
        <CreatePatientModal
          onClose={() => setCreating(false)}
          onCreated={() => void load()}
        />
      )}

      {editing && (
        <EditPatientModal
          patient={editing}
          onClose={() => setEditing(null)}
          onSaved={(updated) => {
            setEditing(null);
            toast("success", `Paciente "${updated.name}" atualizado.`);
            void load();
          }}
        />
      )}

      {deleting && (
        <ConfirmDialog
          title="Excluir paciente"
          message={
            <>
              Excluir <strong>{deleting.name}</strong>? O cartão de vacinação e
              todos os registros associados também serão removidos. Essa ação
              não pode ser desfeita.
            </>
          }
          confirmLabel="Excluir paciente"
          onConfirm={handleDelete}
          onClose={() => setDeleting(null)}
        />
      )}
    </div>
  );
}

function CreatePatientModal({
  onClose,
  onCreated,
}: {
  onClose: () => void;
  onCreated: () => void;
}) {
  const [name, setName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [fieldError, setFieldError] = useState<string | undefined>();
  const [busy, setBusy] = useState(false);
  const [created, setCreated] = useState<CreatedPatient | null>(null);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setFieldError(undefined);

    try {
      const patient = await createPatient(name.trim());
      setCreated(patient);
      onCreated();
    } catch (err) {
      if (err instanceof ApiError) {
        setFieldError(err.fieldError("name"));
        setError(err.details.length > 0 ? null : err.message);
      } else {
        setError("Erro inesperado. Tente novamente.");
      }
    } finally {
      setBusy(false);
    }
  };

  if (created) {
    return (
      <Modal title="Paciente cadastrado" onClose={onClose}>
        <div className="space-y-4">
          <p className="text-sm text-slate-600">
            Credenciais de acesso de <strong>{created.name}</strong>:
          </p>

          <CredentialRow label="Usuário" value={created.username} />
          <CredentialRow label="Senha" value={created.password} />

          <div className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-800">
            ⚠️ A senha é exibida <strong>apenas uma vez</strong>. Copie e
            entregue ao paciente antes de fechar.
          </div>

          <div className="flex justify-end">
            <button type="button" className="btn-primary" onClick={onClose}>
              Concluir
            </button>
          </div>
        </div>
      </Modal>
    );
  }

  return (
    <Modal title="Novo paciente" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <div className="alert-error">{error}</div>}

        <div>
          <label htmlFor="patient-name" className="label">
            Nome
          </label>
          <input
            id="patient-name"
            className="input"
            value={name}
            onChange={(event) => setName(event.target.value)}
            placeholder="ex.: Ana Paula"
            autoFocus
          />
          {fieldError && <p className="field-error">{fieldError}</p>}
        </div>

        <p className="text-xs text-slate-500">
          O usuário e a senha de acesso do paciente são gerados automaticamente.
        </p>

        <div className="flex justify-end gap-2 pt-2">
          <button
            type="button"
            className="btn-secondary"
            onClick={onClose}
            disabled={busy}
          >
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

function EditPatientModal({
  patient,
  onClose,
  onSaved,
}: {
  patient: Patient;
  onClose: () => void;
  onSaved: (patient: Patient) => void;
}) {
  const [name, setName] = useState(patient.name);
  const [error, setError] = useState<string | null>(null);
  const [fieldError, setFieldError] = useState<string | undefined>();
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setFieldError(undefined);

    try {
      const updated = await updatePatient(patient.id, name.trim());
      onSaved(updated);
    } catch (err) {
      if (err instanceof ApiError) {
        setFieldError(err.fieldError("name"));
        setError(err.details.length > 0 ? null : err.message);
      } else {
        setError("Erro inesperado. Tente novamente.");
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <Modal title="Editar paciente" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <div className="alert-error">{error}</div>}

        <div>
          <label htmlFor="edit-patient-name" className="label">
            Nome
          </label>
          <input
            id="edit-patient-name"
            className="input"
            value={name}
            onChange={(event) => setName(event.target.value)}
            autoFocus
          />
          {fieldError && <p className="field-error">{fieldError}</p>}
        </div>

        <p className="text-xs text-slate-500">
          O usuário de acesso não muda ao alterar o nome.
        </p>

        <div className="flex justify-end gap-2 pt-2">
          <button
            type="button"
            className="btn-secondary"
            onClick={onClose}
            disabled={busy}
          >
            Cancelar
          </button>
          <button type="submit" className="btn-primary" disabled={busy}>
            {busy ? "Salvando…" : "Salvar"}
          </button>
        </div>
      </form>
    </Modal>
  );
}

function CredentialRow({ label, value }: { label: string; value: string }) {
  const [copied, setCopied] = useState(false);

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(value);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // Clipboard indisponível (ex.: contexto não seguro): usuário ainda vê o valor.
    }
  };

  return (
    <div>
      <span className="label">{label}</span>
      <div className="flex gap-2">
        <code className="flex-1 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm break-all">
          {value}
        </code>
        <button type="button" className="btn-secondary shrink-0" onClick={copy}>
          {copied ? "Copiado ✓" : "Copiar"}
        </button>
      </div>
    </div>
  );
}
