import { request } from "../lib/api";
import type { Paged, Vaccine } from "../lib/types";

export function listVaccines(page = 1, pageSize = 20): Promise<Paged<Vaccine>> {
  return request<Paged<Vaccine>>(`/vaccines?page=${page}&pageSize=${pageSize}`);
}

/** Busca todas as páginas — usado para montar o cartão (join vaccineId → nome). */
export async function listAllVaccines(): Promise<Vaccine[]> {
  const all: Vaccine[] = [];
  let page = 1;
  let totalPages = 1;

  do {
    const result = await listVaccines(page, 100);
    all.push(...result.items);
    totalPages = result.totalPages;
    page++;
  } while (page <= totalPages);

  return all;
}

export function createVaccine(name: string, totalDoses: number | null): Promise<Vaccine> {
  return request<Vaccine>("/vaccines", {
    method: "POST",
    body: { name, totalDoses },
  });
}
