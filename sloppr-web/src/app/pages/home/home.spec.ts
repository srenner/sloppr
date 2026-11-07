import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import type { MealIdeaDTO, RecipeDTO } from '../../api/model';
import { Home } from './home';

function mealIdea(id: number, name: string): MealIdeaDTO {
  return {
    id,
    name,
    keyIngredients: [{ id, name: 'rice' }],
  };
}

function recipe(mealIdeaId: number): RecipeDTO {
  return {
    id: 0,
    name: 'Chicken Rice Bowl',
    description: 'A quick bowl.',
    servingCount: 2,
    prepTimeMinutes: 10,
    cookTimeMinutes: 20,
    mealIdeaId,
    ingredients: [{ order: 1, quantity: 1, unit: 'cup', name: 'rice' }],
    steps: [{ order: 1, instruction: 'Cook the rice.' }],
  };
}

describe('Home', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
  });

  function setPrompt(value: string): void {
    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    textarea.value = value;
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function submit(): void {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
  }

  function generateIdeas(prompt = 'chicken and rice'): void {
    setPrompt(prompt);
    submit();

    const request = http.expectOne('/api/AiInference/meal-ideas');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ prompt });
    request.flush([mealIdea(1, 'Idea One'), mealIdea(2, 'Idea Two'), mealIdea(3, 'Idea Three')]);
    fixture.detectChanges();
  }

  function openFirstIdea(): RecipeDTO {
    const card = fixture.nativeElement.querySelector(
      '[data-testid="idea-card"]',
    ) as HTMLButtonElement;
    card.click();

    const request = http.expectOne('/api/AiInference/recipes/1');
    expect(request.request.method).toBe('POST');
    const preview = recipe(1);
    request.flush(preview);
    fixture.detectChanges();
    return preview;
  }

  function click(testId: string): void {
    const button = fixture.nativeElement.querySelector(
      `[data-testid="${testId}"]`,
    ) as HTMLButtonElement;
    expect(button).toBeTruthy();
    button.click();
    fixture.detectChanges();
  }

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('sends the prompt and renders the ideas', () => {
    generateIdeas();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Idea One');
    expect(text).toContain('Idea Two');
    expect(text).toContain('Idea Three');
    expect(text).toContain('You asked: "chicken and rice"');
  });

  it('shows a retryable error when no ideas come back', () => {
    setPrompt('chicken');
    submit();

    http.expectOne('/api/AiInference/meal-ideas').flush([]);
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alert.textContent).toContain('No meal ideas');
    expect(fixture.nativeElement.querySelector('textarea')).toBeTruthy();
  });

  it('previews a recipe when an idea is clicked', () => {
    generateIdeas();
    openFirstIdea();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Chicken Rice Bowl');
    expect(text).toContain('Cook the rice.');
    expect(text).toContain('Save and cook tonight');
  });

  it('saves and marks the recipe for tonight', () => {
    generateIdeas();
    openFirstIdea();
    click('save-cook');

    const save = http.expectOne('/api/Recipes');
    expect(save.request.method).toBe('POST');
    expect(save.request.body.name).toBe('Chicken Rice Bowl');
    expect(save.request.body.mealIdeaId).toBe(1);
    save.flush({ ...recipe(1), id: 9 });

    const approval = http.expectOne('/api/MealIdeas/1/approval');
    expect(approval.request.method).toBe('PUT');
    expect(approval.request.body).toEqual({ isUserApproved: true });
    approval.flush(null, { status: 204, statusText: 'No Content' });

    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain("Tonight's dinner");
    expect(fixture.nativeElement.querySelector('[data-testid="start-over"]')).toBeTruthy();
  });

  it('saves for later and continues with the remaining ideas', () => {
    generateIdeas();
    openFirstIdea();
    click('save-later');

    http.expectOne('/api/Recipes').flush({ ...recipe(1), id: 9 });
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Idea Two');
    expect(text).toContain('Saved for later');
  });

  it('rejects a recipe without saving it', () => {
    generateIdeas();
    openFirstIdea();
    click('reject-recipe');

    const approval = http.expectOne('/api/MealIdeas/1/approval');
    expect(approval.request.method).toBe('PUT');
    expect(approval.request.body).toEqual({ isUserApproved: false });
    approval.flush(null, { status: 204, statusText: 'No Content' });
    fixture.detectChanges();

    http.expectNone('/api/Recipes');

    const text = fixture.nativeElement.textContent as string;
    expect(text).not.toContain('Idea One');
    expect(text).toContain('Idea Two');
  });

  it('rejects all pending ideas and restores the prompt', () => {
    generateIdeas();
    click('reject-all');

    const requests = http.match(
      (request) => request.method === 'PUT' && request.url.endsWith('/approval'),
    );
    expect(requests.length).toBe(3);
    requests.forEach((request) => {
      expect(request.request.body).toEqual({ isUserApproved: false });
      request.flush(null, { status: 204, statusText: 'No Content' });
    });
    fixture.detectChanges();

    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    expect(textarea).toBeTruthy();
    expect(textarea.value).toBe('chicken and rice');
    expect(fixture.nativeElement.querySelector('[data-testid="idea-card"]')).toBeNull();
  });
});
