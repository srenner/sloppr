import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { finalize, forkJoin } from 'rxjs';
import { AiInferenceService } from '../../api/ai-inference/ai-inference.service';
import { MealIdeasService } from '../../api/meal-ideas/meal-ideas.service';
import { RecipesService } from '../../api/recipes/recipes.service';
import type {
  MealIdeaDTO,
  RecipeDTO,
  RecipeIngredientDTO,
  RecipeRequest,
} from '../../api/model';

type Phase = 'input' | 'ideas' | 'recipe' | 'done';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  private readonly aiInference = inject(AiInferenceService);
  private readonly mealIdeas = inject(MealIdeasService);
  private readonly recipes = inject(RecipesService);

  private readonly promptInput = viewChild<ElementRef<HTMLTextAreaElement>>('promptInput');

  protected readonly phase = signal<Phase>('input');
  protected readonly prompt = signal('');
  protected readonly lastPrompt = signal<string | null>(null);
  protected readonly ideas = signal<MealIdeaDTO[]>([]);
  protected readonly activeIdea = signal<MealIdeaDTO | null>(null);
  protected readonly recipe = signal<RecipeDTO | null>(null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly savedIdeaIds = signal<ReadonlySet<string>>(new Set());

  protected readonly canSend = computed(
    () => this.prompt().trim().length > 0 && !this.loading() && !this.saving(),
  );

  protected readonly canRejectAll = computed(() =>
    this.ideas().some((idea) => !this.isSaved(idea)),
  );

  protected onInput(event: Event): void {
    this.prompt.set((event.target as HTMLTextAreaElement).value);
  }

  protected send(): void {
    const text = this.prompt().trim();
    if (!text || this.loading() || this.saving()) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.aiInference
      .postApiAiInferenceMealIdeas({ prompt: text }, 'application/json')
      .subscribe({
        next: (ideas) => {
          this.loading.set(false);
          const usable = ideas.filter((idea) => this.ideaKey(idea) !== null);
          if (usable.length === 0) {
            this.error.set(
              'No meal ideas came back. Check that the AI provider is running, then try again.',
            );
            return;
          }

          this.lastPrompt.set(text);
          this.prompt.set('');
          this.ideas.set(usable);
          this.savedIdeaIds.set(new Set());
          this.activeIdea.set(null);
          this.recipe.set(null);
          this.phase.set('ideas');
        },
        error: () => {
          this.loading.set(false);
          this.error.set(
            "Couldn't generate meal ideas. Check that the API and AI provider are running, then try again.",
          );
        },
      });
  }

  protected openRecipe(idea: MealIdeaDTO): void {
    const id = this.ideaKey(idea);
    if (!id || this.loading() || this.saving() || this.isSaved(idea)) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.activeIdea.set(idea);
    this.recipe.set(null);
    this.phase.set('recipe');

    this.aiInference
      .postApiAiInferenceRecipesMealIdeaId(id, 'application/json')
      .subscribe({
        next: (recipe) => {
          this.loading.set(false);
          this.recipe.set(recipe);
        },
        error: () => {
          this.loading.set(false);
          this.activeIdea.set(null);
          this.phase.set('ideas');
          this.error.set("Couldn't generate a recipe for that idea. Try again or pick another.");
        },
      });
  }

  protected backToIdeas(): void {
    if (this.loading() || this.saving()) {
      return;
    }

    this.error.set(null);
    this.activeIdea.set(null);
    this.recipe.set(null);
    this.phase.set('ideas');
  }

  protected saveAndCook(): void {
    this.persistRecipe(true);
  }

  protected saveForLater(): void {
    this.persistRecipe(false);
  }

  protected rejectRecipe(): void {
    const idea = this.activeIdea();
    const id = idea ? this.ideaKey(idea) : null;
    if (!id || this.saving()) {
      return;
    }

    this.error.set(null);
    this.saving.set(true);
    this.removeIdea(id);

    this.mealIdeas
      .putApiMealIdeasIdApproval(id, { isUserApproved: false })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        error: () => this.error.set('Idea removed, but the rejection was not recorded.'),
      });
  }

  protected rejectAll(): void {
    const ids = this.ideas()
      .filter((idea) => !this.isSaved(idea))
      .map((idea) => this.ideaKey(idea))
      .filter((id): id is string => id !== null);

    if (ids.length === 0) {
      return;
    }

    this.error.set(null);
    this.saving.set(true);
    this.resetToInput();

    forkJoin(
      ids.map((id) => this.mealIdeas.putApiMealIdeasIdApproval(id, { isUserApproved: false })),
    )
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        error: () =>
          this.error.set('Ideas cleared, but the rejections were not all recorded.'),
      });
  }

  protected startOver(): void {
    this.prompt.set('');
    this.lastPrompt.set(null);
    this.error.set(null);
    this.resetToInput();
  }

  protected isSaved(idea: MealIdeaDTO): boolean {
    const id = this.ideaKey(idea);
    return id !== null && this.savedIdeaIds().has(id);
  }

  protected ingredientLabel(ingredient: RecipeIngredientDTO): string {
    const amount = [ingredient.quantity, ingredient.unit]
      .filter((part) => part !== null && part !== undefined && part !== '')
      .join(' ');
    const note = ingredient.note ? ` (${ingredient.note})` : '';
    return `${amount ? `${amount} ` : ''}${ingredient.name}${note}`;
  }

  private persistRecipe(cookTonight: boolean): void {
    const idea = this.activeIdea();
    const recipe = this.recipe();
    const id = idea ? this.ideaKey(idea) : null;
    if (!idea || !recipe || !id || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const onPersisted = (): void => {
      this.markSaved(idea);

      if (!cookTonight) {
        this.saving.set(false);
        this.returnToIdeas();
        return;
      }

      this.mealIdeas
        .putApiMealIdeasIdApproval(id, { isUserApproved: true })
        .subscribe({
          next: () => {
            this.saving.set(false);
            this.phase.set('done');
          },
          error: () => {
            this.saving.set(false);
            this.error.set(
              'Recipe saved, but it could not be marked for tonight. Try again.',
            );
          },
        });
    };

    if (this.isSaved(idea)) {
      onPersisted();
      return;
    }

    this.recipes.postApiRecipes(this.toRequest(recipe, id), 'application/json').subscribe({
      next: onPersisted,
      error: () => {
        this.saving.set(false);
        this.error.set("Couldn't save the recipe. Try again.");
      },
    });
  }

  private toRequest(recipe: RecipeDTO, mealIdeaId: string): RecipeRequest {
    return {
      name: recipe.name,
      description: recipe.description,
      servingCount: Number(recipe.servingCount ?? 0),
      prepTimeMinutes: Number(recipe.prepTimeMinutes ?? 0),
      cookTimeMinutes: Number(recipe.cookTimeMinutes ?? 0),
      difficulty: this.toNumberOrNull(recipe.difficulty),
      notes: recipe.notes ?? null,
      mealIdeaId: Number(mealIdeaId),
      ingredients: (recipe.ingredients ?? []).map((ingredient) => ({
        order: Number(ingredient.order ?? 0),
        quantity: this.toNumberOrNull(ingredient.quantity),
        unit: ingredient.unit ?? null,
        name: ingredient.name,
        group: ingredient.group ?? null,
        note: ingredient.note ?? null,
      })),
      steps: (recipe.steps ?? []).map((step) => ({
        order: Number(step.order ?? 0),
        instruction: step.instruction,
        group: step.group ?? null,
      })),
    };
  }

  private toNumberOrNull(value: number | string | null | undefined): number | null {
    if (value === null || value === undefined) {
      return null;
    }

    const parsed = Number(value);
    return Number.isNaN(parsed) ? null : parsed;
  }

  private markSaved(idea: MealIdeaDTO): void {
    const id = this.ideaKey(idea);
    if (!id) {
      return;
    }

    const next = new Set(this.savedIdeaIds());
    next.add(id);
    this.savedIdeaIds.set(next);
  }

  private removeIdea(id: string): void {
    this.ideas.set(this.ideas().filter((idea) => this.ideaKey(idea) !== id));
    this.returnToIdeas();
  }

  private returnToIdeas(): void {
    this.activeIdea.set(null);
    this.recipe.set(null);

    if (this.ideas().length > 0) {
      this.phase.set('ideas');
    } else {
      this.phase.set('input');
      this.restorePrompt();
    }

    this.focusPrompt();
  }

  private resetToInput(): void {
    this.phase.set('input');
    this.activeIdea.set(null);
    this.recipe.set(null);
    this.ideas.set([]);
    this.savedIdeaIds.set(new Set());
    this.restorePrompt();
    this.focusPrompt();
  }

  private restorePrompt(): void {
    const last = this.lastPrompt();
    if (last && this.prompt().trim().length === 0) {
      this.prompt.set(last);
    }
  }

  private focusPrompt(): void {
    setTimeout(() => this.promptInput()?.nativeElement.focus());
  }

  private ideaKey(idea: MealIdeaDTO): string | null {
    return idea.id == null ? null : String(idea.id);
  }
}
