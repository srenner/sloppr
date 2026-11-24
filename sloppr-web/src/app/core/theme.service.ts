import { DOCUMENT, Injectable, computed, inject, signal } from '@angular/core';

export type ThemeMode = 'system' | 'light' | 'dark';
export type ResolvedTheme = 'light' | 'dark';

export const THEME_STORAGE_KEY = 'sloppr-theme';

const DARK_QUERY = '(prefers-color-scheme: dark)';
const THEME_COLORS: Record<ResolvedTheme, string> = {
  light: '#ffffff',
  dark: '#030712',
};

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly window = this.document.defaultView;
  private readonly mediaQuery = this.window?.matchMedia?.(DARK_QUERY) ?? null;
  private readonly systemPrefersDark = signal(this.mediaQuery?.matches ?? false);

  private readonly modeState = signal<ThemeMode>(this.readStoredMode());

  /** Stored preference: follow the OS, or force a theme. */
  readonly mode = this.modeState.asReadonly();

  /** Theme actually applied to the page. */
  readonly resolved = computed<ResolvedTheme>(() => {
    const mode = this.mode();
    if (mode === 'system') {
      return this.systemPrefersDark() ? 'dark' : 'light';
    }
    return mode;
  });

  constructor() {
    this.apply(this.resolved());

    this.mediaQuery?.addEventListener('change', (event) => {
      this.systemPrefersDark.set(event.matches);
      if (this.mode() === 'system') {
        this.apply(this.resolved());
      }
    });

    // Keep multiple tabs in sync.
    this.window?.addEventListener('storage', (event) => {
      if (event.key !== THEME_STORAGE_KEY) {
        return;
      }
      this.modeState.set(this.readStoredMode());
      this.apply(this.resolved());
    });
  }

  setMode(mode: ThemeMode): void {
    this.modeState.set(mode);
    this.writeStoredMode(mode);
    this.apply(this.resolved());
  }

  /** Cycles System → Light → Dark. */
  cycle(): void {
    const order: ThemeMode[] = ['system', 'light', 'dark'];
    const next = order[(order.indexOf(this.mode()) + 1) % order.length];
    this.setMode(next);
  }

  private apply(theme: ResolvedTheme): void {
    const root = this.document.documentElement;
    root.classList.toggle('dark', theme === 'dark');

    const meta = this.document.querySelector('meta[name="theme-color"]');
    meta?.setAttribute('content', THEME_COLORS[theme]);
  }

  private readStoredMode(): ThemeMode {
    try {
      const stored = this.window?.localStorage.getItem(THEME_STORAGE_KEY);
      if (stored === 'light' || stored === 'dark' || stored === 'system') {
        return stored;
      }
    } catch {
      // Storage can be unavailable; fall back to the OS preference.
    }
    return 'system';
  }

  private writeStoredMode(mode: ThemeMode): void {
    try {
      this.window?.localStorage.setItem(THEME_STORAGE_KEY, mode);
    } catch {
      // Ignore write failures; the in-memory preference still applies.
    }
  }
}
