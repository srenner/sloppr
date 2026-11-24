import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import type { CookbookEntryDTO } from '../../api/model';
import { CookbookEntry } from './cookbook-entry';

function cookbookEntry(): CookbookEntryDTO {
  return {
    id: 10,
    cookbookId: 1,
    sourceRecipeId: 7,
    currentVersionNumber: 1,
    content: {
      name: 'Chicken Rice Bowl',
      description: 'A quick bowl.',
      servingCount: 2,
      prepTimeMinutes: 5,
      cookTimeMinutes: 10,
      difficulty: 2,
      ingredients: [],
      steps: [],
    },
    dateCreated: '2026-01-01T00:00:00Z',
    dateUpdated: '2026-01-01T00:00:00Z',
  };
}

describe('CookbookEntry', () => {
  let fixture: ComponentFixture<CookbookEntry>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CookbookEntry],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ cookbookId: '1', entryId: '10' }),
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CookbookEntry);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  it('renders the cookbook entry as a full recipe', () => {
    const entryRequest = http.expectOne('/api/Cookbooks/1/entries/10');
    expect(entryRequest.request.method).toBe('GET');
    entryRequest.flush(cookbookEntry());

    const markdownRequest = http.expectOne('/api/Recipes/markdown');
    expect(markdownRequest.request.method).toBe('POST');
    expect(markdownRequest.request.body.content.name).toBe('Chicken Rice Bowl');
    markdownRequest.flush('# Chicken Rice Bowl\n\nA quick bowl.');
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Chicken Rice Bowl');
    expect(text).toContain('A quick bowl.');
  });

  it('shows an error when the entry cannot be loaded', () => {
    http
      .expectOne('/api/Cookbooks/1/entries/10')
      .flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alert.textContent).toContain("Couldn't load that recipe");
  });
});
