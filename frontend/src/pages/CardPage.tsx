import { useCallback, useEffect, useMemo, useState } from "react";
import type { FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import {
  getPatient,
  registerVaccination,
  removeVaccination,
} from "../api/patients";
import { listAllVaccines } from "../api/vaccines";
import { useAuth } from "../auth/AuthContext";
import { ConfirmDialog } from "../components/ConfirmDialog";
import { Modal } from "../components/Modal";
import { useToast } from "../components/Toast";
import { ApiError } from "../lib/api";
import { formatDate, todayIso } from "../lib/format";
import type { Patient, Vaccination, Vaccine } from "../lib/types";

interface VaccineCardModel {
  vaccine: Vaccine;
  applied: Vaccination[];
  nextDose: number;
  isComplete: boolean;
}

export function CardPage() {
  const { id } = useParams<{ id: string }>();
  const { isAdmin } = useAuth();
  const { toast } = useToast();

  const [patient, setPatient] = useState<Patient | null>(null);
  const [vaccines, setVaccines] = useState<Vaccine[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [registering, setRegistering] = useState<{ vaccineId?: string } | null>(
    null,
  );
  const [removing, setRemoving] = useState<{
    vaccination: Vaccination;
    vaccineName: string;
  } | null>(null);

  const load = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setLoadError(null);
    try {
      const [patientResult, vaccinesResult] = await Promise.all([
        getPatient(id),
        listAllVaccines(),
      ]);
      setPatient(patientResult);
      setVaccines(vaccinesResult);
    } catch (err) {
      setLoadError(
        err instanceof ApiError ? err.message : "Erro ao carregar o cartão.",
      );
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    void load();
  }, [load]);

  const cards = useMemo<VaccineCardModel[]>(() => {
    if (!patient) return [];
    return vaccines.map((vaccine) => {
      const applied = patient.vaccinations
        .filter((vaccination) => vaccination.vaccineId === vaccine.id)
        .sort((a, b) => a.dose - b.dose);
      const nextDose =
        applied.length === 0 ? 1 : Math.max(...applied.map((v) => v.dose)) + 1;
      const isComplete =
        vaccine.totalDoses != null && nextDose > vaccine.totalDoses;
      return { vaccine, applied, nextDose, isComplete };
    });
  }, [patient, vaccines]);

  const orphanRecords = useMemo(() => {
    if (!patient) return [];
    const known = new Set(vaccines.map((vaccine) => vaccine.id));
    return patient.vaccinations.filter(
      (vaccination) => !known.has(vaccination.vaccineId),
    );
  }, [patient, vaccines]);

  const handleRemove = async () => {
    if (!removing || !id) return;
    try {
      await removeVaccination(id, removing.vaccination.id);
      toast("success", "Registro de vacinação removido.");
      setRemoving(null);
      void load();
    } catch (err) {
      setRemoving(null);
      toast(
        "error",
        err instanceof ApiError ? err.message : "Erro ao remover registro.",
      );
    }
  };

  if (loading) {
    return (
      <p className="py-16 text-center text-slate-400">Carregando cartão…</p>
    );
  }

  if (loadError || !patient) {
    return (
      <div className="mx-auto max-w-md py-16 text-center">
        <div className="alert-error">
          {loadError ?? "Paciente não encontrado."}
        </div>
        {isAdmin && (
          <Link to="/patients" className="btn-secondary mt-4">
            ← Voltar para pacientes
          </Link>
        )}
      </div>
    );
  }

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <div>
          {isAdmin && (
            <Link
              to="/patients"
              className="text-sm text-teal-700 hover:underline"
            >
              ← Pacientes
            </Link>
          )}
          <h1 className="text-2xl font-bold text-slate-900">{patient.name}</h1>
          <p className="mt-1 text-sm text-slate-500">
            {patient.vaccinations.length}{" "}
            {patient.vaccinations.length === 1
              ? "vacinação registrada"
              : "vacinações registradas"}
          </p>
        </div>
        <button
          type="button"
          className="btn-primary"
          onClick={() => setRegistering({})}
        >
          + Registrar vacinação
        </button>
      </div>

      {vaccines.length === 0 ? (
        <div className="card p-8 text-center text-slate-400">
          Nenhuma vacina cadastrada no sistema.
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {cards.map((card) => (
            <VaccineCard
              key={card.vaccine.id}
              card={card}
              onRegister={() => setRegistering({ vaccineId: card.vaccine.id })}
              onRemove={(vaccination) =>
                setRemoving({ vaccination, vaccineName: card.vaccine.name })
              }
            />
          ))}
        </div>
      )}

      {orphanRecords.length > 0 && (
        <div className="card mt-4 p-4">
          <h2 className="mb-2 font-semibold text-slate-800">
            Outros registros
          </h2>
          <p className="mb-3 text-xs text-slate-500">
            Registros de vacinas que não estão mais no catálogo.
          </p>
          <div className="flex flex-wrap gap-2">
            {orphanRecords.map((vaccination) => (
              <DoseChip
                key={vaccination.id}
                vaccination={vaccination}
                onRemove={() =>
                  setRemoving({
                    vaccination,
                    vaccineName: "vacina desconhecida",
                  })
                }
              />
            ))}
          </div>
        </div>
      )}

      {registering && (
        <RegisterModal
          patientId={patient.id}
          cards={cards}
          initialVaccineId={registering.vaccineId}
          onClose={() => setRegistering(null)}
          onRegistered={(updated) => {
            setRegistering(null);
            setPatient(updated);
            toast("success", "Vacinação registrada.");
          }}
        />
      )}

      {removing && (
        <ConfirmDialog
          title="Remover registro"
          message={
            <>
              Remover a <strong>{removing.vaccination.dose}ª dose</strong> de{" "}
              <strong>{removing.vaccineName}</strong>, aplicada em{" "}
              {formatDate(removing.vaccination.applicationDate)}?
            </>
          }
          confirmLabel="Remover"
          onConfirm={handleRemove}
          onClose={() => setRemoving(null)}
        />
      )}
    </div>
  );
}

function VaccineCard({
  card,
  onRegister,
  onRemove,
}: {
  card: VaccineCardModel;
  onRegister: () => void;
  onRemove: (vaccination: Vaccination) => void;
}) {
  const { vaccine, applied, nextDose, isComplete } = card;
  const isPeriodic = vaccine.totalDoses == null;

  return (
    <div className="card flex flex-col p-4">
      <div className="mb-3 flex items-start justify-between gap-2">
        <h2 className="font-semibold text-slate-800">{vaccine.name}</h2>
        {isComplete ? (
          <span className="shrink-0 rounded-full bg-teal-100 px-2.5 py-0.5 text-xs font-semibold text-teal-700">
            ✓ Completa
          </span>
        ) : isPeriodic ? (
          <span className="shrink-0 rounded-full bg-sky-100 px-2.5 py-0.5 text-xs font-semibold text-sky-700">
            Periódica
          </span>
        ) : (
          <span className="shrink-0 text-xs font-medium text-slate-400">
            {applied.length}/{vaccine.totalDoses}
          </span>
        )}
      </div>

      {!isPeriodic && vaccine.totalDoses != null && (
        <div className="mb-3 h-1.5 overflow-hidden rounded-full bg-slate-100">
          <div
            className="h-full rounded-full bg-teal-500 transition-all"
            style={{
              width: `${Math.min(100, (applied.length / vaccine.totalDoses) * 100)}%`,
            }}
          />
        </div>
      )}

      <div className="flex flex-wrap gap-2">
        {applied.map((vaccination) => (
          <DoseChip
            key={vaccination.id}
            vaccination={vaccination}
            onRemove={() => onRemove(vaccination)}
          />
        ))}

        {!isComplete && (
          <button
            type="button"
            onClick={onRegister}
            className="rounded-lg border border-dashed border-teal-400 px-2.5 py-1.5 text-xs font-semibold text-teal-700 transition-colors hover:bg-teal-50"
          >
            + {nextDose}ª dose
          </button>
        )}

        {!isPeriodic &&
          vaccine.totalDoses != null &&
          !isComplete &&
          Array.from({ length: vaccine.totalDoses - nextDose }, (_, index) => (
            <span
              key={index}
              className="rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs text-slate-300"
            >
              {nextDose + index + 1}ª dose
            </span>
          ))}
      </div>
    </div>
  );
}

function DoseChip({
  vaccination,
  onRemove,
}: {
  vaccination: Vaccination;
  onRemove: () => void;
}) {
  return (
    <span className="group inline-flex items-center gap-1.5 rounded-lg bg-teal-600/10 py-1.5 pr-1.5 pl-2.5 text-xs font-medium text-teal-900">
      <span className="font-semibold">{vaccination.dose}ª</span>
      <span className="text-teal-800/70">
        {formatDate(vaccination.applicationDate)}
      </span>
      <button
        type="button"
        onClick={onRemove}
        aria-label={`Remover ${vaccination.dose}ª dose`}
        title="Remover registro"
        className="rounded p-0.5 text-teal-700/50 transition-colors hover:bg-red-100 hover:text-red-600"
      >
        ✕
      </button>
    </span>
  );
}

function RegisterModal({
  patientId,
  cards,
  initialVaccineId,
  onClose,
  onRegistered,
}: {
  patientId: string;
  cards: VaccineCardModel[];
  initialVaccineId?: string;
  onClose: () => void;
  onRegistered: (patient: Patient) => void;
}) {
  const firstAvailable = cards.find((card) => !card.isComplete);
  const initialCard =
    cards.find((card) => card.vaccine.id === initialVaccineId) ??
    firstAvailable;

  const [vaccineId, setVaccineId] = useState(initialCard?.vaccine.id ?? "");
  const [dose, setDose] = useState(String(initialCard?.nextDose ?? 1));
  const [date, setDate] = useState(todayIso());
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<{
    dose?: string;
    date?: string;
  }>({});
  const [busy, setBusy] = useState(false);

  const selected = cards.find((card) => card.vaccine.id === vaccineId);

  const handleVaccineChange = (newId: string) => {
    setVaccineId(newId);
    const card = cards.find((c) => c.vaccine.id === newId);
    if (card) setDose(String(card.nextDose));
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setFieldErrors({});

    try {
      const updated = await registerVaccination(
        patientId,
        vaccineId,
        Number(dose),
        date,
      );
      onRegistered(updated);
    } catch (err) {
      if (err instanceof ApiError) {
        setFieldErrors({
          dose: err.fieldError("dose"),
          date: err.fieldError("applicationDate"),
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
    <Modal title="Registrar vacinação" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <div className="alert-error">{error}</div>}

        <div>
          <label htmlFor="reg-vaccine" className="label">
            Vacina
          </label>
          <select
            id="reg-vaccine"
            className="input"
            value={vaccineId}
            onChange={(event) => handleVaccineChange(event.target.value)}
          >
            {cards.map((card) => (
              <option
                key={card.vaccine.id}
                value={card.vaccine.id}
                disabled={card.isComplete}
              >
                {card.vaccine.name}
                {card.isComplete ? " (completa)" : ""}
              </option>
            ))}
          </select>
          {selected && (
            <p className="mt-1 text-xs text-slate-500">
              {selected.vaccine.totalDoses == null
                ? `Vacina periódica — próxima dose: ${selected.nextDose}ª`
                : `Esquema de ${selected.vaccine.totalDoses} ${selected.vaccine.totalDoses === 1 ? "dose" : "doses"} — próxima: ${selected.nextDose}ª`}
            </p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label htmlFor="reg-dose" className="label">
              Dose
            </label>
            <input
              id="reg-dose"
              type="number"
              min={1}
              className="input"
              value={dose}
              onChange={(event) => setDose(event.target.value)}
            />
            {fieldErrors.dose && (
              <p className="field-error">{fieldErrors.dose}</p>
            )}
          </div>
          <div>
            <label htmlFor="reg-date" className="label">
              Data de aplicação
            </label>
            <input
              id="reg-date"
              type="date"
              className="input"
              value={date}
              max={todayIso()}
              onChange={(event) => setDate(event.target.value)}
            />
            {fieldErrors.date && (
              <p className="field-error">{fieldErrors.date}</p>
            )}
          </div>
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <button
            type="button"
            className="btn-secondary"
            onClick={onClose}
            disabled={busy}
          >
            Cancelar
          </button>
          <button
            type="submit"
            className="btn-primary"
            disabled={busy || !vaccineId}
          >
            {busy ? "Registrando…" : "Registrar"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
