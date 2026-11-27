import type { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { AiInferenceService } from '../../api/ai-inference/ai-inference.service';
import { CookbooksService } from '../../api/cookbooks/cookbooks.service';
import { MealIdeasService } from '../../api/meal-ideas/meal-ideas.service';
import { RecipeSourceType } from '../../api/model';
import { RecipesService } from '../../api/recipes/recipes.service';
import { MarkdownView } from '../../components/markdown-view/markdown-view';
import type { MealIdeaDTO, RecipeDTO, RecipeRequest } from '../../api/model';

type Phase = 'input' | 'ideas' | 'recipe' | 'imported' | 'done';

const MAX_IMAGE_BYTES = 10 * 1024 * 1024;

@Component({
  selector: 'app-home',
  imports: [MarkdownView, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  private readonly aiInference = inject(AiInferenceService);
  private readonly cookbooks = inject(CookbooksService);
  private readonly mealIdeas = inject(MealIdeasService);
  private readonly recipes = inject(RecipesService);

  private readonly promptInput = viewChild<ElementRef<HTMLTextAreaElement>>('promptInput');

  protected readonly phase = signal<Phase>('input');
  protected readonly prompt = signal('');
  protected readonly lastPrompt = signal<string | null>(null);
  protected readonly ideas = signal<MealIdeaDTO[]>([]);
  protected readonly activeIdea = signal<MealIdeaDTO | null>(null);
  protected readonly recipe = signal<RecipeDTO | null>(null);
  protected readonly recipeMarkdown = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly importing = signal(false);
  protected readonly importMode = signal(false);
  protected readonly dragging = signal(false);
  protected readonly imagePreview = signal<string | null>(null);
  protected readonly importedRecipeId = signal<number | null>(null);
  protected readonly cookbookAddError = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly savedIdeaIds = signal<ReadonlySet<string>>(new Set());

  protected readonly canSend = computed(
    () => this.prompt().trim().length > 0 && !this.loading() && !this.saving(),
  );

  protected readonly canRejectAll = computed(() =>
    this.ideas().some((idea) => !this.isSaved(idea)),
  );

  protected readonly recipeTitle = computed(
    () => this.recipe()?.content?.name?.trim() || this.activeIdea()?.name?.trim() || null,
  );

  protected onInput(event: Event): void {
    this.prompt.set((event.target as HTMLTextAreaElement).value);
  }

  protected onPaste(event: ClipboardEvent): void {
    if (!this.canImportImage()) {
      return;
    }

    const image = this.imageFromClipboard(event.clipboardData);
    if (!image) {
      return;
    }

    event.preventDefault();
    this.importImageFile(image);
  }

  protected onDragOver(event: DragEvent): void {
    if (!this.hasFiles(event.dataTransfer)) {
      return;
    }

    event.preventDefault();

    if (this.canImportImage()) {
      this.dragging.set(true);
    }
  }

  protected onDragLeave(event: DragEvent): void {
    const related = event.relatedTarget as Node | null;
    const current = event.currentTarget as HTMLElement | null;

    if (related instanceof Node && current?.contains(related)) {
      return;
    }

    this.dragging.set(false);
  }

  protected onDrop(event: DragEvent): void {
    if (!this.hasFiles(event.dataTransfer)) {
      return;
    }

    event.preventDefault();
    this.dragging.set(false);

    if (!this.canImportImage()) {
      return;
    }

    const image = this.imageFromFiles(event.dataTransfer?.files);
    if (!image) {
      this.error.set('Drop an image file, like a photo of a recipe.');
      return;
    }

    this.importImageFile(image);
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter' || event.shiftKey || event.isComposing) {
      return;
    }

    event.preventDefault();
    if (this.canSend()) {
      this.send();
    }
  }

  protected send(): void {
    const text = this.prompt().trim();
    if (!text || this.loading() || this.saving()) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.aiInference.postApiAiInferenceMealIdeas({ prompt: text }, 'application/json').subscribe({
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
    this.recipeMarkdown.set(null);
    this.phase.set('recipe');

    this.aiInference.postApiAiInferenceRecipesMealIdeaId(Number(id), 'application/json').subscribe({
      next: (recipe) => {
        this.recipe.set(recipe);
        this.loadRecipeMarkdown(recipe);
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
    this.recipeMarkdown.set(null);
    this.phase.set('ideas');
  }

  protected backFromRecipe(): void {
    if (this.importMode()) {
      this.discardImport();
      return;
    }

    this.backToIdeas();
  }

  protected saveAndCook(): void {
    this.persistRecipe(true);
  }

  protected saveForLater(): void {
    this.persistRecipe(false);
  }

  protected saveImport(): void {
    const recipe = this.recipe();
    if (!recipe || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.cookbookAddError.set(false);

    this.recipes
      .postApiRecipes(this.toRequest(recipe, null, RecipeSourceType.Import), 'application/json')
      .subscribe({
        next: (saved) => {
          const recipeId = this.numericId(saved.id);
          this.importedRecipeId.set(recipeId);

          if (recipeId === null) {
            this.finishImport(false);
            return;
          }

          this.addImportToCookbook(recipeId);
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error.set(this.saveImportErrorMessage(error));
        },
      });
  }

  protected discardImport(): void {
    if (this.loading() || this.saving()) {
      return;
    }

    this.resetImportState();
    this.activeIdea.set(null);
    this.recipe.set(null);
    this.recipeMarkdown.set(null);
    this.phase.set('input');
    this.error.set(null);
    this.focusPrompt();
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
        error: () => this.error.set('Ideas cleared, but the rejections were not all recorded.'),
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

  private loadRecipeMarkdown(recipe: RecipeDTO): void {
    this.recipes.postApiRecipesMarkdown(recipe).subscribe({
      next: (markdown) => {
        this.loading.set(false);
        this.importing.set(false);
        this.recipeMarkdown.set(markdown);
      },
      error: () => {
        this.loading.set(false);
        this.importing.set(false);
        this.activeIdea.set(null);
        this.recipe.set(null);
        this.recipeMarkdown.set(null);

        if (this.importMode()) {
          this.resetImportState();
          this.phase.set('input');
          this.error.set("Couldn't preview that imported recipe. Try pasting the photo again.");
          return;
        }

        this.phase.set('ideas');
        this.error.set("Couldn't format that recipe. Try again or pick another.");
      },
    });
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

      this.mealIdeas.putApiMealIdeasIdApproval(id, { isUserApproved: true }).subscribe({
        next: () => {
          this.saving.set(false);
          this.phase.set('done');
        },
        error: () => {
          this.saving.set(false);
          this.error.set('Recipe saved, but it could not be marked for tonight. Try again.');
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

  private toRequest(
    recipe: RecipeDTO,
    mealIdeaId: string | null,
    sourceType: RecipeSourceType | null = null,
  ): RecipeRequest {
    return {
      mealIdeaId: mealIdeaId === null ? null : Number(mealIdeaId),
      sourceType,
      content: {
        name: recipe.content.name,
        description: recipe.content.description,
        servingCount: Number(recipe.content.servingCount ?? 0),
        prepTimeMinutes: Number(recipe.content.prepTimeMinutes ?? 0),
        cookTimeMinutes: Number(recipe.content.cookTimeMinutes ?? 0),
        difficulty: this.toNumberOrNull(recipe.content.difficulty),
        notes: recipe.content.notes ?? null,
        ingredients: (recipe.content.ingredients ?? []).map((ingredient) => ({
          quantity: this.toNumberOrNull(ingredient.quantity),
          unit: ingredient.unit ?? null,
          name: ingredient.name ?? '',
          group: ingredient.group ?? null,
          note: ingredient.note ?? null,
        })),
        steps: (recipe.content.steps ?? [])
          .filter((step) => (step.instruction ?? '').trim().length > 0)
          .map((step) => ({
            instruction: step.instruction ?? '',
            group: step.group ?? null,
          })),
      },
    };
  }

  private startImageImport(imageBase64: string, contentType: string): void {
    this.loading.set(true);
    this.importing.set(true);
    this.error.set(null);
    this.activeIdea.set(null);
    this.recipe.set(null);
    this.recipeMarkdown.set(null);
    this.importedRecipeId.set(null);
    this.cookbookAddError.set(false);

    this.aiInference
      .postApiAiInferenceRecipesFromImage({ imageBase64, contentType }, 'application/json')
      .subscribe({
        next: (recipe) => {
          this.recipe.set(recipe);
          this.importMode.set(true);
          this.phase.set('recipe');
          this.loadRecipeMarkdown(recipe);
        },
        error: (error: HttpErrorResponse) => {
          this.loading.set(false);
          this.importing.set(false);
          this.imagePreview.set(null);
          this.error.set(this.importErrorMessage(error));
        },
      });
  }

  private canImportImage(): boolean {
    return this.phase() === 'input' && !this.loading() && !this.saving();
  }

  private hasFiles(data: DataTransfer | null): boolean {
    return data ? Array.from(data.types).includes('Files') : false;
  }

  private imageFromClipboard(data: DataTransfer | null): File | null {
    if (!data) {
      return null;
    }

    for (let index = 0; index < data.items.length; index++) {
      const item = data.items[index];
      if (item.kind !== 'file') {
        continue;
      }

      const file = item.getAsFile();
      if (file && (file.type.startsWith('image/') || item.type.startsWith('image/'))) {
        return file;
      }
    }

    return null;
  }

  private imageFromFiles(files: FileList | null | undefined): File | null {
    if (!files) {
      return null;
    }

    for (let index = 0; index < files.length; index++) {
      const file = files[index];
      if (file.type.startsWith('image/')) {
        return file;
      }
    }

    return null;
  }

  private importImageFile(image: File): void {
    this.error.set(null);
    this.dragging.set(false);

    if (image.size > MAX_IMAGE_BYTES) {
      this.error.set('That photo is too large. Use an image under 10 MB.');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const dataUrl = typeof reader.result === 'string' ? reader.result : '';
      const base64 = dataUrl.split(',')[1] ?? '';
      const contentType = image.type || 'image/png';

      this.imagePreview.set(dataUrl);
      this.startImageImport(base64, contentType);
    };
    reader.onerror = () => {
      this.error.set("Couldn't read that photo. Try another image.");
    };
    reader.readAsDataURL(image);
  }

  private saveImportErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      const errors = (error.error as { errors?: Record<string, string[]> } | null)?.errors;
      const first = errors ? Object.values(errors).flat()[0] : null;
      if (first) {
        return `Couldn't save the imported recipe: ${first}`;
      }
    }

    return "Couldn't save the imported recipe. Try again.";
  }

  private importErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return (
        'Photo import is not set up. Choose models for Recipe Image Scanner and ' +
        'Recipe Generation in Settings.'
      );
    }

    if (error.status === 502) {
      return "Couldn't read a recipe from that photo. Try a clearer image.";
    }

    return (
      "Couldn't import that photo. Check that the API and AI provider are running, " +
      'then try again.'
    );
  }

  private addImportToCookbook(recipeId: number): void {
    this.cookbooks.getApiCookbooks('application/json').subscribe({
      next: (cookbooks) => {
        const family =
          cookbooks.find((item) => item.name === 'Family Cookbook') ?? cookbooks[0];
        const cookbookId = this.numericId(family?.id);
        if (cookbookId === null) {
          this.finishImport(false);
          return;
        }

        this.cookbooks
          .postApiCookbooksCookbookIdEntriesFromRecipeRecipeId(
            cookbookId,
            recipeId,
            'application/json',
          )
          .subscribe({
            next: () => this.finishImport(true),
            error: () => this.finishImport(false),
          });
      },
      error: () => this.finishImport(false),
    });
  }

  private finishImport(addedToCookbook: boolean): void {
    this.saving.set(false);
    this.cookbookAddError.set(!addedToCookbook);
    this.phase.set('imported');
  }

  private resetImportState(): void {
    this.importing.set(false);
    this.importMode.set(false);
    this.dragging.set(false);
    this.imagePreview.set(null);
    this.importedRecipeId.set(null);
    this.cookbookAddError.set(false);
  }

  private numericId(value: number | string | null | undefined): number | null {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const id = Number(value);
    return Number.isInteger(id) && id > 0 ? id : null;
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
    this.recipeMarkdown.set(null);

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
    this.recipeMarkdown.set(null);
    this.ideas.set([]);
    this.savedIdeaIds.set(new Set());
    this.resetImportState();
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
