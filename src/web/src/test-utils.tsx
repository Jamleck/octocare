import { render, type RenderOptions } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { DevAuthProvider } from '@/providers/DevAuthProvider';
import type { ReactElement } from 'react';

function AllProviders({ children }: { children: React.ReactNode }) {
  return (
    <DevAuthProvider>
      <MemoryRouter>{children}</MemoryRouter>
    </DevAuthProvider>
  );
}

export function renderWithProviders(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>,
) {
  return render(ui, { wrapper: AllProviders, ...options });
}

export { screen, waitFor, within } from '@testing-library/react';
export { default as userEvent } from '@testing-library/user-event';
