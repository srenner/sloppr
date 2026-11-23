import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import type { RecipePageDTO, RecipeSummaryDTO } from '../../api/model';
import { Recipes } from './recipes';

class NoopIntersectionObserver implements IntersectionObserver {
  readonly root: Element | Document | null = null;
  readonly rootMargin = '';
  readonly thresholds: ReadonlyArray<number> = [];

  disconnect(): void {}
  observe(): void {}
  takeRecords(): IntersectionObserverEntry[] {
    return [];
  }
  unobserve(): void {}
}

function summary(id: number, name: string): RecipeSummaryDTO {
  return {
    id,
    name,
    description: `${name} description`,
    sourceType: 'Manual',
    servingCount: 2,
    prepTimeMinutes: 5,
    cookTimeMinutes: 10,
    difficulty: 1,
    dateUpdated: '2026-01-01T00:00:00Z',
  };
}

function page(items: RecipeSummaryDTO[], total: number, pageNumber: number): RecipePageDTO {
  return { items, total, page: pageNumber, pageSize: 24 };
}

describe('Recipes', () => {
  let component: Recipes;
  let fixture: ComponentFixture<Recipes>;
  let http: HttpTestingController;

  beforeEach(async () => {
    globalThis.IntersectionObserver =
      NoopIntersectionObserver as unknown as typeof IntersectionObserver;

    await TestBed.configureTestingModule({
      imports: [Recipes],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Recipes);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  function flushPage(items: RecipeSummaryDTO[], total: number, pageNumber: number): void {
    const request = http.expectOne((req) => req.url === '/api/Recipes/paged');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('page')).toBe(String(pageNumber));
    expect(request.request.params.get('pageSize')).toBe('24');
    request.flush(page(items, total, pageNumber));
    fixture.detectChanges();
  }

  it('loads the first page', () => {
    flushPage([summary(1, 'Chicken Rice Bowl'), summary(2, 'Tomato Soup')], 2, 1);

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Chicken Rice Bowl');
    expect(text).toContain('Tomato Soup');
    expect(text).toContain('2 of 2');
  });

  it('appends the next page when Load more is clicked', () => {
    flushPage([summary(1, 'One'), summary(2, 'Two')], 3, 1);

    const button = fixture.nativeElement.querySelector(
      '[data-testid="load-more"]',
    ) as HTMLButtonElement;
    expect(button).toBeTruthy();
    button.click();
    fixture.detectChanges();

    flushPage([summary(3, 'Three')], 3, 2);

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Three');
    expect(text).toContain('3 of 3');
    expect(text).toContain("You've reached the end.");
  });

  it('shows an error with a retry that reloads the same page', () => {
    http
      .expectOne((req) => req.url === '/api/Recipes/paged')
      .flush(null, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alert.textContent).toContain("Couldn't load recipes");

    (fixture.nativeElement.querySelector('[data-testid="retry"]') as HTMLButtonElement).click();
    fixture.detectChanges();

    flushPage([summary(1, 'Recovered')], 1, 1);
    expect(fixture.nativeElement.textContent).toContain('Recovered');
  });
});
