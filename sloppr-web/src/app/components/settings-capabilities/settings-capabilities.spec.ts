import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import type { AiCapabilitySettingDTO, AiModelDTO } from '../../api/model';
import { SettingsCapabilities } from './settings-capabilities';

describe('SettingsCapabilities', () => {
  let component: SettingsCapabilities;
  let fixture: ComponentFixture<SettingsCapabilities>;
  let http: HttpTestingController;

  const models: AiModelDTO[] = [
    {
      id: 1,
      name: 'Granite 4.1 8B',
      identifier: 'granite4.1:8b',
      aiProviderId: 1,
      aiProvider: { id: 1, name: 'Ollama' },
      isActive: true,
    },
    {
      id: 2,
      name: 'Llama 3.2',
      identifier: 'llama3.2:3b',
      aiProviderId: 1,
      aiProvider: { id: 1, name: 'Ollama' },
      isActive: true,
    },
  ];

  const settings: AiCapabilitySettingDTO[] = [
    {
      capability: 'IngredientExtraction',
      modelId: 2,
      systemPrompt: 'Extract ingredients.',
    },
    {
      capability: 'IdeaGeneration',
      modelId: 1,
      systemPrompt: 'Plan meals.',
    },
    {
      capability: 'RecipeGeneration',
      modelId: 1,
      systemPrompt: 'Write recipes.',
    },
    {
      capability: 'OcrScanning',
      modelId: 2,
      systemPrompt: 'Read recipes.',
    },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsCapabilities],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsCapabilities);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  function initialise(currentSettings: AiCapabilitySettingDTO[] = settings): void {
    http.expectOne('/api/AiModels').flush(models);
    http.expectOne('/api/AiCapabilitySettings').flush(currentSettings);
    fixture.detectChanges();
  }

  function select(index: number): HTMLSelectElement {
    return fixture.nativeElement.querySelectorAll('select')[index] as HTMLSelectElement;
  }

  function textarea(index: number): HTMLTextAreaElement {
    return fixture.nativeElement.querySelectorAll('textarea')[index] as HTMLTextAreaElement;
  }

  function choose(index: number, value: string): void {
    const element = select(index);
    element.value = value;
    element.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }

  function type(index: number, value: string): void {
    const element = textarea(index);
    element.value = value;
    element.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function button(label: string): HTMLButtonElement {
    const match = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((candidate) => candidate.textContent?.trim() === label);
    expect(match).toBeTruthy();
    return match as HTMLButtonElement;
  }

  it('should load the current capabilities', async () => {
    initialise();
    await fixture.whenStable();

    expect(select(0).value).toBe('2');
    expect(select(1).value).toBe('1');
    expect(select(2).value).toBe('1');
    expect(select(3).value).toBe('2');
    expect(textarea(0).value).toBe('Extract ingredients.');
    expect(textarea(1).value).toBe('Plan meals.');
    expect(fixture.nativeElement.textContent).toContain('Granite 4.1 8B');
    expect(fixture.nativeElement.textContent).toContain('Recipe Image Scanner');
  });

  it('should clear preferred models that are no longer active', () => {
    initialise(
      settings.map((setting) =>
        setting.capability === 'OcrScanning' ? { ...setting, modelId: 99 } : setting,
      ),
    );

    expect(select(3).value).toBe('');
  });

  it('should disable save until every prompt is filled in', () => {
    initialise(
      settings.map((setting) =>
        setting.capability === 'IdeaGeneration' ? { ...setting, systemPrompt: '' } : setting,
      ),
    );

    expect(button('Save capabilities').disabled).toBe(true);

    type(1, 'Plan meals.');
    expect(button('Save capabilities').disabled).toBe(false);
  });

  it('should save the selected models and prompts', async () => {
    initialise();

    choose(0, '1');
    type(0, 'Extract ingredients and cuisines.');
    button('Save capabilities').click();

    const request = http.expectOne('/api/AiCapabilitySettings');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual([
      {
        capability: 'IngredientExtraction',
        modelId: 1,
        systemPrompt: 'Extract ingredients and cuisines.',
      },
      { capability: 'IdeaGeneration', modelId: 1, systemPrompt: 'Plan meals.' },
      { capability: 'RecipeGeneration', modelId: 1, systemPrompt: 'Write recipes.' },
      { capability: 'OcrScanning', modelId: 2, systemPrompt: 'Read recipes.' },
    ]);

    request.flush(
      settings.map((setting) =>
        setting.capability === 'IngredientExtraction'
          ? { ...setting, modelId: 1, systemPrompt: 'Extract ingredients and cuisines.' }
          : setting,
      ),
    );
    fixture.detectChanges();
    await fixture.whenStable();

    expect(select(0).value).toBe('1');
    expect(fixture.nativeElement.textContent).toContain('Capability defaults saved.');
  });
});
