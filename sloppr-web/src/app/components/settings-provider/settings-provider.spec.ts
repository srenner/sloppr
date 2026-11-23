import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import type { AiProviderDTO } from '../../api/model';
import { SettingsProvider } from './settings-provider';

describe('SettingsProvider', () => {
  let component: SettingsProvider;
  let fixture: ComponentFixture<SettingsProvider>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsProvider],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsProvider);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  function provider(overrides: Partial<AiProviderDTO> = {}): AiProviderDTO {
    return {
      id: 1,
      name: 'Ollama',
      providerType: 'Ollama',
      baseUrl: 'http://localhost:11434',
      isHealthy: true,
      ...overrides,
    };
  }

  function button(label: string): HTMLButtonElement {
    const match = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((candidate) => candidate.textContent?.trim() === label);
    expect(match).toBeTruthy();
    return match as HTMLButtonElement;
  }

  function submitForm(): void {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
  }

  it('should render providers and their health', async () => {
    http.expectOne('/api/AiProviders').flush([provider()]);
    fixture.detectChanges();
    await fixture.whenStable();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Ollama');
    expect(text).toContain('http://localhost:11434');
    expect(text).toContain('Healthy');
  });

  it('should add an Ollama provider', async () => {
    http.expectOne('/api/AiProviders').flush([]);
    fixture.detectChanges();

    submitForm();

    const request = http.expectOne('/api/AiProviders');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      name: 'Ollama',
      providerType: 'Ollama',
      baseUrl: 'http://localhost:11434',
      isActive: true,
    });

    request.flush(provider());
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Provider added.');
    expect(fixture.nativeElement.textContent).toContain('Healthy');
  });

  it('should show the health failure when the provider cannot be reached', async () => {
    http.expectOne('/api/AiProviders').flush([]);
    fixture.detectChanges();

    submitForm();

    const request = http.expectOne('/api/AiProviders');
    request.flush(
      provider({
        id: 0,
        isHealthy: false,
        lastHealthStatusCode: 0,
        lastHealthResponse: 'Error: Connection refused',
      }),
      { status: 502, statusText: 'Bad Gateway' },
    );
    fixture.detectChanges();
    await fixture.whenStable();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain("Couldn't reach Ollama");
    expect(text).toContain('Error: Connection refused');
  });

  it('should recheck provider health', async () => {
    http.expectOne('/api/AiProviders').flush([provider({ isHealthy: false })]);
    fixture.detectChanges();

    button('Recheck').click();

    const request = http.expectOne('/api/AiProviders/1/health');
    expect(request.request.method).toBe('POST');
    request.flush({ id: 1, isHealthy: true, lastHealthStatusCode: 200 });
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Healthy');
  });
});
