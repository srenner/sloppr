import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import type { AiModelDTO, AiProviderDTO } from '../../api/model';
import { SettingsModels } from './settings-models';

describe('SettingsModels', () => {
  let component: SettingsModels;
  let fixture: ComponentFixture<SettingsModels>;
  let http: HttpTestingController;

  const provider: AiProviderDTO = {
    id: 1,
    name: 'Ollama',
    providerType: 'Ollama',
    baseUrl: 'http://localhost:11434',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsModels],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsModels);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  function model(overrides: Partial<AiModelDTO> = {}): AiModelDTO {
    return {
      id: 1,
      name: 'Granite 4.1 8B',
      identifier: 'granite4.1:8b',
      aiProviderId: 1,
      aiProvider: { id: 1, name: 'Ollama' },
      isActive: true,
      ...overrides,
    };
  }

  function initialise(active: AiModelDTO[] = []): void {
    http.expectOne('/api/AiProviders').flush([provider]);
    http.expectOne('/api/AiModels').flush(active);
    http.expectOne('/api/AiModels/inactive').flush([]);
    fixture.detectChanges();
  }

  function button(label: string): HTMLButtonElement {
    const match = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((candidate) => candidate.textContent?.trim() === label);
    expect(match).toBeTruthy();
    return match as HTMLButtonElement;
  }

  it('should render models in use', async () => {
    initialise([model()]);
    await fixture.whenStable();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Granite 4.1 8B');
    expect(text).toContain('granite4.1:8b');
  });

  it('should discover and add selected models as active', async () => {
    initialise();

    button('Discover models').click();
    fixture.detectChanges();

    const discover = http.expectOne('/api/AiProviders/1/discover-models');
    expect(discover.request.method).toBe('GET');
    discover.flush([
      { name: 'Granite 4.1 8B', identifier: 'granite4.1:8b' },
      { name: 'Llama 3.2', identifier: 'llama3.2:3b' },
    ]);
    fixture.detectChanges();

    const checkbox = fixture.nativeElement.querySelector(
      'input[type="checkbox"]',
    ) as HTMLInputElement;
    checkbox.click();
    fixture.detectChanges();

    button('Add selected (1)').click();

    const batch = http.expectOne('/api/AiModels/batch');
    expect(batch.request.method).toBe('POST');
    expect(batch.request.body).toEqual([
      {
        name: 'Granite 4.1 8B',
        identifier: 'granite4.1:8b',
        aiProviderId: 1,
        contextWindow: null,
        capabilities: null,
        isActive: true,
      },
    ]);
    batch.flush(1);

    http.expectOne('/api/AiModels').flush([model()]);
    http.expectOne('/api/AiModels/inactive').flush([]);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Models added and ready to use.');
  });

  it('should stop using an active model', async () => {
    initialise([model()]);

    button('Stop using').click();

    const request = http.expectOne('/api/AiModels/1');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      name: 'Granite 4.1 8B',
      identifier: 'granite4.1:8b',
      aiProviderId: 1,
      contextWindow: null,
      capabilities: null,
      isActive: false,
    });
    request.flush(null);

    http.expectOne('/api/AiModels').flush([]);
    http.expectOne('/api/AiModels/inactive').flush([model({ isActive: false })]);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('No models are in use yet.');
    expect(fixture.nativeElement.textContent).toContain('Available models');
  });
});
