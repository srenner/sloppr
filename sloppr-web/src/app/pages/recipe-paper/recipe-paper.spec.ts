import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import type { CookbookEntryDTO, CookbookEntrySummaryDTO } from '../../api/model';
import { RecipePaper } from './recipe-paper';

function cookbookEntrySummary(id: number, sourceRecipeId: number): CookbookEntrySummaryDTO {
  return {
    id,
    cookbookId: 1,
    sourceRecipeId,
    currentVersionNumber: 1,
    name: 'Chicken Rice Bowl',
    description: 'A quick bowl.',
    servingCount: 2,
    prepTimeMinutes: 5,
    cookTimeMinutes: 10,
    difficulty: 2,
    dateCreated: '2026-01-01T00:00:00Z',
    dateUpdated: '2026-01-01T00:00:00Z',
  };
}

function cookbookEntry(id: number, sourceRecipeId: number): CookbookEntryDTO {
  return {
    id,
    cookbookId: 1,
    sourceRecipeId,
    currentVersionNumber: 1,
    content: { name: 'Chicken Rice Bowl', description: 'A quick bowl.' },
    dateCreated: '2026-01-01T00:00:00Z',
    dateUpdated: '2026-01-01T00:00:00Z',
  };
}

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

  function flushMarkdown(): void {
    http.expectOne('/api/Recipes/7/markdown').flush('# Chicken Rice Bowl\n\nA quick bowl.');
  }

  function flushCookbook(entries: CookbookEntrySummaryDTO[]): void {
    http.expectOne('/api/Cookbooks').flush([
      {
        id: 1,
        name: 'Family Cookbook',
        dateCreated: '2026-01-01T00:00:00Z',
        dateUpdated: '2026-01-01T00:00:00Z',
      },
    ]);
    http.expectOne('/api/Cookbooks/1/entries').flush(entries);
    fixture.detectChanges();
  }

  it('renders the recipe markdown', () => {
    flushMarkdown();
    flushCookbook([]);

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

  it('adds the recipe to the family cookbook', () => {
    flushMarkdown();
    flushCookbook([]);

    const add = fixture.nativeElement.querySelector(
      '[data-testid="add-to-cookbook"]',
    ) as HTMLButtonElement;
    expect(add).toBeTruthy();
    add.click();

    const request = http.expectOne('/api/Cookbooks/1/entries/from-recipe/7');
    expect(request.request.method).toBe('POST');
    request.flush(cookbookEntry(10, 7));
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('In your family cookbook');
    expect(
      fixture.nativeElement.querySelector('[data-testid="remove-from-cookbook"]'),
    ).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[data-testid="add-to-cookbook"]')).toBeNull();
  });

  it('shows remove when the recipe is already in the family cookbook', () => {
    flushMarkdown();
    flushCookbook([cookbookEntrySummary(10, 7)]);

    expect(
      fixture.nativeElement.querySelector('[data-testid="remove-from-cookbook"]'),
    ).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[data-testid="add-to-cookbook"]')).toBeNull();
  });

  it('removes the recipe from the family cookbook', () => {
    flushMarkdown();
    flushCookbook([cookbookEntrySummary(10, 7)]);

    (fixture.nativeElement.querySelector(
      '[data-testid="remove-from-cookbook"]',
    ) as HTMLButtonElement).click();

    const request = http.expectOne(
      (req) => req.method === 'DELETE' && req.url === '/api/Cookbooks/1/entries/10',
    );
    request.flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[data-testid="add-to-cookbook"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('[data-testid="remove-from-cookbook"]')).toBeNull();
  });

  it('shows an error when adding to the cookbook fails', () => {
    flushMarkdown();
    flushCookbook([]);

    (fixture.nativeElement.querySelector(
      '[data-testid="add-to-cookbook"]',
    ) as HTMLButtonElement).click();

    http
      .expectOne('/api/Cookbooks/1/entries/from-recipe/7')
      .flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alert.textContent).toContain("Couldn't add that recipe to your cookbook");
  });
});
