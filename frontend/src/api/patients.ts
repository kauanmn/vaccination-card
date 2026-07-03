import { request } from "../lib/api";
import type { CreatedPatient, Paged, Patient } from "../lib/types";

export function listPatients(page = 1, pageSize = 20): Promise<Paged<Patient>> {
  return request<Paged<Patient>>(`/patients?page=${page}&pageSize=${pageSize}`);
}

export function getPatient(id: string): Promise<Patient> {
  return request<Patient>(`/patients/${id}`);
}

export function createPatient(name: string): Promise<CreatedPatient> {
  return request<CreatedPatient>("/patients", {
    method: "POST",
    body: { name },
  });
}

export function deletePatient(id: string): Promise<void> {
  return request<void>(`/patients/${id}`, { method: "DELETE" });
}

export function registerVaccination(
  patientId: string,
  vaccineId: string,
  dose: number,
  applicationDate: string,
): Promise<Patient> {
  return request<Patient>(`/patients/${patientId}/vaccinations`, {
    method: "POST",
    body: { vaccineId, dose, applicationDate },
  });
}

export function removeVaccination(patientId: string, vaccinationId: string): Promise<void> {
  return request<void>(`/patients/${patientId}/vaccinations/${vaccinationId}`, {
    method: "DELETE",
  });
}
