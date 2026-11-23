import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import type { RecipeSummaryDTO } from '../../api/model';
import { RecipeCard } from './recipe-card';

describe('RecipeCard', () => {
  let fixture: ComponentFixture<RecipeCard>;

  const recipe: RecipeSummaryDTO = {
    id: 7,
    name: 'Chicken Rice Bowl',
    description: 'A quick bowl.',
    sourceType: 'Manual',
    servingCount: 2,
    prepTimeMinutes: 5,
    cookTimeMinutes: 10,
    difficulty: 2,
    dateUpdated: '2026-01-01T00:00:00Z',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecipeCard],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(RecipeCard);
    fixture.componentRef.setInput('recipe', recipe);
    fixture.detectChanges();
  });

  it('renders the summary and links to the recipe', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Chicken Rice Bowl');
    expect(text).toContain('A quick bowl.');
    expect(text).toContain('Serves 2');
    expect(text).toContain('Medium');

    const link = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/recipes/7');
  });
});
