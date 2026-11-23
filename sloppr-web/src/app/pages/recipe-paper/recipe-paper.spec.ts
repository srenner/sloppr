import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { RecipePaper } from './recipe-paper';

describe('RecipePaper', () => {
  let fixture: ComponentFixture<RecipePaper>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecipePaper],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '7' }) } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RecipePaper);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  it('renders the recipe markdown', () => {
    const request = http.expectOne('/api/Recipes/7/markdown');
    expect(request.request.method).toBe('GET');
    request.flush('# Chicken Rice Bowl\n\nA quick bowl.');
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Chicken Rice Bowl');
    expect(text).toContain('A quick bowl.');
  });

  it('shows an error when the recipe cannot be loaded', () => {
    http
      .expectOne('/api/Recipes/7/markdown')
      .flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alert.textContent).toContain("Couldn't load that recipe");
  });
});
