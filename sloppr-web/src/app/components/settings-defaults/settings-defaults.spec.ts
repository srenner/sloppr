import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import type { AiModelDTO, ApplicationSettingDTO } from '../../api/model';
import { SettingsDefaults } from './settings-defaults';

describe('SettingsDefaults', () => {
  let component: SettingsDefaults;
  let fixture: ComponentFixture<SettingsDefaults>;
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

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsDefaults],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsDefaults);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  function initialise(settings: ApplicationSettingDTO = {}): void {
    http.expectOne('/api/AiModels').flush(models);
    http.expectOne('/api/ApplicationSettings').flush(settings);
    fixture.detectChanges();
  }

  function select(index: number): HTMLSelectElement {
    return fixture.nativeElement.querySelectorAll('select')[index] as HTMLSelectElement;
  }

  function choose(index: number, value: string): void {
    const element = select(index);
    element.value = value;
    element.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }

  function button(label: string): HTMLButtonElement {
    const match = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((candidate) => candidate.textContent?.trim() === label);
    expect(match).toBeTruthy();
    return match as HTMLButtonElement;
  }

  it('should load the current defaults', async () => {
    initialise({ extractionModelId: 2, ideaModelId: 1, recipeModelId: 1, ocrModelId: 2 });
    await fixture.whenStable();

    expect(select(0).value).toBe('2');
    expect(select(1).value).toBe('1');
    expect(select(2).value).toBe('1');
    expect(select(3).value).toBe('2');
    expect(fixture.nativeElement.textContent).toContain('Granite 4.1 8B');
  });

  it('should disable save until a model is selected', () => {
    initialise();

    expect(button('Save defaults').disabled).toBe(true);

    choose(0, '1');
    expect(button('Save defaults').disabled).toBe(false);
  });

  it('should save the selected defaults as model ids', async () => {
    initialise();

    choose(0, '2');
    choose(2, '1');
    choose(3, '2');
    button('Save defaults').click();

    const request = http.expectOne('/api/ApplicationSettings');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      extractionModelId: 2,
      ideaModelId: null,
      recipeModelId: 1,
      ocrModelId: 2,
    });

    request.flush({ extractionModelId: 2, ideaModelId: null, recipeModelId: 1, ocrModelId: 2 });
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Default models saved.');
  });
});
