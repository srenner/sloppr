import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
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
    mealIdeaId,
    content: {
      name: 'Chicken Rice Bowl',
      description: 'A quick bowl.',
      servingCount: 2,
      prepTimeMinutes: 10,
      cookTimeMinutes: 20,
      ingredients: [{ quantity: 1, unit: 'cup', name: 'rice' }],
      steps: [{ instruction: '', group: 'Preparation' }, { instruction: 'Cook the rice.' }],
    },
  };
}

describe('Home', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
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

    const markdown = http.expectOne('/api/Recipes/markdown');
    expect(markdown.request.method).toBe('POST');
    markdown.flush(
      `# ${preview.content.name}\n\n${preview.content.description}\n\n1. Cook the rice.`,
    );
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

  async function pasteImage(): Promise<void> {
    const file = new File(['png'], 'recipe.png', { type: 'image/png' });
    const event = new Event('paste', { bubbles: true, cancelable: true });
    Object.defineProperty(event, 'clipboardData', {
      value: {
        items: [
          {
            kind: 'file',
            type: 'image/png',
            getAsFile: () => file,
          },
        ],
      },
    });

    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    textarea.dispatchEvent(event);
    await new Promise((resolve) => setTimeout(resolve, 10));
    fixture.detectChanges();
  }

  function dropEvent(type: 'dragover' | 'dragleave' | 'drop', file?: File): Event {
    const event = new Event(type, { bubbles: true, cancelable: true });
    Object.defineProperty(event, 'dataTransfer', {
      value: {
        files: file ? [file] : [],
        types: file ? ['Files'] : [],
      },
    });
    return event;
  }

  async function dropImage(): Promise<void> {
    const file = new File(['png'], 'recipe.png', { type: 'image/png' });
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(dropEvent('drop', file));
    await new Promise((resolve) => setTimeout(resolve, 10));
    fixture.detectChanges();
  }

  async function importRecipe(): Promise<RecipeDTO> {
    await pasteImage();

    const request = http.expectOne('/api/AiInference/recipes/from-image');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.contentType).toBe('image/png');
    expect(typeof request.request.body.imageBase64).toBe('string');
    expect(request.request.body.imageBase64.length).toBeGreaterThan(0);

    const imported: RecipeDTO = {
      ...recipe(0),
      mealIdeaId: null,
      sourceType: 'Import',
    };
    request.flush(imported);
    fixture.detectChanges();

    const markdown = http.expectOne('/api/Recipes/markdown');
    expect(markdown.request.method).toBe('POST');
    markdown.flush(`# ${imported.content.name}\n\n1. Cook the rice.`);
    fixture.detectChanges();

    return imported;
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

  it('shows the idea name while the recipe is generating', () => {
    generateIdeas();

    const card = fixture.nativeElement.querySelector(
      '[data-testid="idea-card"]',
    ) as HTMLButtonElement;
    card.click();
    fixture.detectChanges();

    const request = http.expectOne('/api/AiInference/recipes/1');
    expect(request.request.method).toBe('POST');

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Idea One');
    expect(text).toContain('Writing the recipe…');

    request.flush(recipe(1));
    fixture.detectChanges();

    const generated = fixture.nativeElement.textContent as string;
    expect(generated).toContain('Chicken Rice Bowl');
    expect(generated).toContain('Writing the recipe…');

    http.expectOne('/api/Recipes/markdown').flush('# Chicken Rice Bowl');
    fixture.detectChanges();
  });

  it('saves and marks the recipe for tonight', () => {
    generateIdeas();
    openFirstIdea();
    click('save-cook');

    const save = http.expectOne('/api/Recipes');
    expect(save.request.method).toBe('POST');
    expect(save.request.body.content.name).toBe('Chicken Rice Bowl');
    expect(save.request.body.content.steps).toEqual([
      { instruction: 'Cook the rice.', group: null },
    ]);
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

  it('imports a pasted image and saves it to the family cookbook', async () => {
    const imported = await importRecipe();

    expect(fixture.nativeElement.textContent).toContain('Save to family cookbook');

    click('save-import');

    const save = http.expectOne('/api/Recipes');
    expect(save.request.method).toBe('POST');
    expect(save.request.body.sourceType).toBe('Import');
    expect(save.request.body.mealIdeaId).toBeNull();
    save.flush({ ...imported, id: 9 });
    fixture.detectChanges();

    const cookbooks = http.expectOne('/api/Cookbooks');
    cookbooks.flush([{ id: 1, name: 'Family Cookbook' }]);
    fixture.detectChanges();

    const add = http.expectOne('/api/Cookbooks/1/entries/from-recipe/9');
    add.flush({
      id: 5,
      cookbookId: 1,
      sourceRecipeId: 9,
      currentVersionNumber: 1,
      content: imported.content,
    });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('added to your family cookbook');
  });

  it('explains when photo import is not configured', async () => {
    await pasteImage();

    http
      .expectOne('/api/AiInference/recipes/from-image')
      .flush('No model', { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]') as HTMLElement;
    expect(alert.textContent).toContain('not set up');
    expect(fixture.nativeElement.querySelector('textarea')).toBeTruthy();
  });

  it('leaves non-image pastes alone', () => {
    const event = new Event('paste', { bubbles: true, cancelable: true });
    Object.defineProperty(event, 'clipboardData', {
      value: {
        items: [{ kind: 'string', type: 'text/plain', getAsFile: () => null }],
      },
    });

    const textarea = fixture.nativeElement.querySelector('textarea') as HTMLTextAreaElement;
    textarea.dispatchEvent(event);
    fixture.detectChanges();

    http.expectNone('/api/AiInference/recipes/from-image');
  });

  it('imports an image dropped on the idea box', async () => {
    await dropImage();

    const request = http.expectOne('/api/AiInference/recipes/from-image');
    const imported: RecipeDTO = {
      ...recipe(0),
      mealIdeaId: null,
      sourceType: 'Import',
    };
    request.flush(imported);
    fixture.detectChanges();

    http.expectOne('/api/Recipes/markdown').flush('# Imported Recipe');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Save to family cookbook');
  });

  it('shows a drop target while dragging a file over the idea box', () => {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    const file = new File(['png'], 'recipe.png', { type: 'image/png' });

    form.dispatchEvent(dropEvent('dragover', file));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Drop a recipe photo to import');

    form.dispatchEvent(dropEvent('dragleave'));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('Drop a recipe photo to import');
  });

  it('rejects non-image drops', () => {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    const file = new File(['notes'], 'notes.txt', { type: 'text/plain' });

    form.dispatchEvent(dropEvent('drop', file));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Drop an image file');
    http.expectNone('/api/AiInference/recipes/from-image');
  });
});
