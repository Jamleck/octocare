import type { ProblemDetails } from '@/types/api';

export class ApiError extends Error {
  status: number;
  statusText: string;
  problem: ProblemDetails | null | undefined;

  constructor(
    status: number,
    statusText: string,
    problem?: ProblemDetails | null,
  ) {
    super(problem?.detail ?? `API error: ${status} ${statusText}`);
    this.status = status;
    this.statusText = statusText;
    this.problem = problem;
  }

  get validationErrors(): Record<string, string[]> | undefined {
    return this.problem?.errors;
  }
}
