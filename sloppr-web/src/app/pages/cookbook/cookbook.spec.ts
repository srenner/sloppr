import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import type { CookbookEntrySummaryDTO } from '../../api/model';
import { Cookbook } from './cookbook';

function entry(id: number, name: string): CookbookEntrySummaryDTO {
  return {
    id,
    cookbookId: 1,
    sourceRecipeId: id + 100,
    currentVersionNumber: 1,
    name,
    description: `${name} description`,
    servingCount: 4,
    prepTimeMinutes: 10,
    cookTimeMinutes: 20,
    difficulty: 2,
    dateCreated: '2026-01-01T00:00:00Z',
    dateUpdated: '2026-01-01T00:00:00Z',
  };
}

describe('Cookbook', () => {
  let fixture: ComponentFixture<Cookbook>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Cookbook],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Cookbook);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

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

  it('loads the family cookbook and its entries', () => {
    flushCookbook([entry(10, 'Chicken Rice Bowl'), entry(11, 'Tomato Soup')]);

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Family Cookbook');
    expect(text).toContain('2 recipes');
    expect(text).toContain('Chicken Rice Bowl');
    expect(text).toContain('Tomato Soup');
  });

  it('links entries to the full cookbook recipe', () => {
    flushCookbook([entry(10, 'Chicken Rice Bowl')]);

    const link = fixture.nativeElement.querySelector(
      '[data-testid="cookbook-entry"] a',
    ) as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.getAttribute('href')).toBe('/cookbook/1/entries/10');
  });

  it('removes an entry from the cookbook', () => {
    flushCookbook([entry(10, 'Chicken Rice Bowl')]);

    const button = fixture.nativeElement.querySelector(
      '[data-testid="remove-entry"]',
    ) as HTMLButtonElement;
    expect(button).toBeTruthy();
    button.click();

    const request = http.expectOne(
      (req) => req.method === 'DELETE' && req.url === '/api/Cookbooks/1/entries/10',
    );
    request.flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).not.toContain('Chicken Rice Bowl');
    expect(text).toContain('No recipes in your cookbook yet');
  });

  it('shows an empty state', () => {
    flushCookbook([]);

    expect(fixture.nativeElement.textContent).toContain('No recipes in your cookbook yet');
  });

  it('shows an error with a retry that reloads the cookbook', () => {
    http
      .expectOne('/api/Cookbooks')
      .flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alert.textContent).toContain("Couldn't load your cookbook");

    (fixture.nativeElement.querySelector('[data-testid="retry"]') as HTMLButtonElement).click();
    fixture.detectChanges();

    flushCookbook([entry(10, 'Recovered')]);
    expect(fixture.nativeElement.textContent).toContain('Recovered');
  });
});
