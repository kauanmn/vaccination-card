export type Role = "Admin" | "Patient";

export interface LoginResponse {
  token: string;
  role: Role;
  expiresAt: string;
}

export interface Vaccine {
  id: string;
  name: string;
  /** null/ausente = vacina periódica, sem limite de doses. */
  totalDoses?: number | null;
}

export interface Vaccination {
  id: string;
  vaccineId: string;
  dose: number;
  /** Formato yyyy-MM-dd. */
  applicationDate: string;
}

export interface Patient {
  id: string;
  name: string;
  vaccinations: Vaccination[];
}

export interface CreatedPatient {
  id: string;
  name: string;
  username: string;
  password: string;
}

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiErrorDetail {
  field: string;
  message: string;
}
