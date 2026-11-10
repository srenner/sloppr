import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import type { AiModelDTO } from '../../api/model';
import { AiModelsListComponent } from './ai-models-list';

describe('AiModelsListComponent', () => {
  let component: AiModelsListComponent;
  let fixture: ComponentFixture<AiModelsListComponent>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiModelsListComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(AiModelsListComponent);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  it('should create', () => {
    http.expectOne('/api/AiModels').flush([]);
    expect(component).toBeTruthy();
  });

  it('should render the discovered models', async () => {
    const request = http.expectOne('/api/AiModels');
    expect(request.request.method).toBe('GET');

    const model: AiModelDTO = {
      id: 1,
      name: 'Granite 4.1 8B',
      identifier: 'granite4.1:8b',
      aiProvider: { name: 'Ollama' },
      contextWindow: 128000,
    };
    request.flush([model]);
    fixture.detectChanges();
    await fixture.whenStable();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Granite 4.1 8B');
    expect(text).toContain('granite4.1:8b');
  });
});
