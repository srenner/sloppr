import { TestBed } from '@angular/core/testing';
import { THEME_STORAGE_KEY, ThemeService } from './theme.service';

interface MatchMediaStub {
  setMatches(matches: boolean): void;
}

function stubMatchMedia(initialMatches: boolean): MatchMediaStub {
  let matches = initialMatches;
  const listeners = new Set<(event: MediaQueryListEvent) => void>();
  const media = {
    get matches() {
      return matches;
    },
    media: '(prefers-color-scheme: dark)',
    onchange: null,
    addEventListener: (_type: string, listener: (event: MediaQueryListEvent) => void) => {
      listeners.add(listener);
    },
    removeEventListener: (_type: string, listener: (event: MediaQueryListEvent) => void) => {
      listeners.delete(listener);
    },
    addListener: () => {},
    removeListener: () => {},
    dispatchEvent: () => true,
  } as unknown as MediaQueryList;

  vi.stubGlobal(
    'matchMedia',
    vi.fn(() => media),
  );

  return {
    setMatches(next: boolean) {
      matches = next;
      const event = { matches: next, media: media.media } as MediaQueryListEvent;
      listeners.forEach((listener) => listener(event));
    },
  };
}

describe('ThemeService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({});
    localStorage.clear();
    document.documentElement.classList.remove('dark');
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    localStorage.clear();
    document.documentElement.classList.remove('dark');
  });

  it('follows the OS preference by default', () => {
    stubMatchMedia(true);

    const service = TestBed.inject(ThemeService);

    expect(service.mode()).toBe('system');
    expect(service.resolved()).toBe('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('updates when the OS preference changes while in system mode', () => {
    const media = stubMatchMedia(false);
    const service = TestBed.inject(ThemeService);

    media.setMatches(true);

    expect(service.resolved()).toBe('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('ignores OS changes once a theme is forced', () => {
    const media = stubMatchMedia(false);
    const service = TestBed.inject(ThemeService);

    service.setMode('light');
    media.setMatches(true);

    expect(service.resolved()).toBe('light');
    expect(document.documentElement.classList.contains('dark')).toBe(false);
  });

  it('persists the selected mode', () => {
    stubMatchMedia(false);
    const service = TestBed.inject(ThemeService);

    service.setMode('dark');

    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('restores a stored mode', () => {
    stubMatchMedia(false);
    localStorage.setItem(THEME_STORAGE_KEY, 'dark');

    const service = TestBed.inject(ThemeService);

    expect(service.mode()).toBe('dark');
    expect(service.resolved()).toBe('dark');
  });

  it('falls back to system for invalid stored values', () => {
    stubMatchMedia(false);
    localStorage.setItem(THEME_STORAGE_KEY, 'neon');

    const service = TestBed.inject(ThemeService);

    expect(service.mode()).toBe('system');
    expect(service.resolved()).toBe('light');
  });

  it('cycles system → light → dark', () => {
    stubMatchMedia(false);
    const service = TestBed.inject(ThemeService);

    service.cycle();
    expect(service.mode()).toBe('light');

    service.cycle();
    expect(service.mode()).toBe('dark');

    service.cycle();
    expect(service.mode()).toBe('system');
  });
});
