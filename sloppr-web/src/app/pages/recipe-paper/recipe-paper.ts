import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CookbooksService } from '../../api/cookbooks/cookbooks.service';
import { RecipesService } from '../../api/recipes/recipes.service';
import { MarkdownView } from '../../components/markdown-view/markdown-view';

@Component({
  selector: 'app-recipe-paper',
  imports: [MarkdownView, RouterLink],
  templateUrl: './recipe-paper.html',
  styleUrl: './recipe-paper.css',
})
export class RecipePaper {
  private readonly recipes = inject(RecipesService);
  private readonly cookbooks = inject(CookbooksService);
  private readonly route = inject(ActivatedRoute);
  private readonly recipeId: number | null;

  protected readonly markdown = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly cookbookId = signal<number | null>(null);
  protected readonly cookbookEntryId = signal<number | null>(null);
  protected readonly cookbookLoading = signal(false);
  protected readonly cookbookSaving = signal(false);
  protected readonly cookbookError = signal<string | null>(null);

  constructor() {
    this.recipeId = this.numericId(this.route.snapshot.paramMap.get('id'));
    if (this.recipeId === null) {
      this.loading.set(false);
      this.error.set('Recipe not found.');
      return;
    }

    this.recipes.getApiRecipesIdMarkdown(String(this.recipeId)).subscribe({
      next: (markdown) => {
        this.markdown.set(markdown);
        this.loading.set(false);
        this.loadCookbookMembership();
      },
      error: () => {
        this.loading.set(false);
        this.error.set("Couldn't load that recipe.");
      },
    });
  }

  protected addToCookbook(): void {
    const cookbookId = this.cookbookId();
    const recipeId = this.recipeId;
    if (cookbookId === null || recipeId === null || this.cookbookSaving()) {
      return;
    }

    this.cookbookSaving.set(true);
    this.cookbookError.set(null);

    this.cookbooks
      .postApiCookbooksCookbookIdEntriesFromRecipeRecipeId(
        cookbookId,
        recipeId,
        'application/json',
      )
      .subscribe({
        next: (entry) => {
          this.cookbookEntryId.set(this.numericId(entry.id));
          this.cookbookSaving.set(false);
        },
        error: () => {
          this.cookbookSaving.set(false);
          this.cookbookError.set("Couldn't add that recipe to your cookbook. Try again.");
        },
      });
  }

  protected removeFromCookbook(): void {
    const cookbookId = this.cookbookId();
    const entryId = this.cookbookEntryId();
    if (cookbookId === null || entryId === null || this.cookbookSaving()) {
      return;
    }

    this.cookbookSaving.set(true);
    this.cookbookError.set(null);

    this.cookbooks.deleteApiCookbooksCookbookIdEntriesEntryId(cookbookId, entryId).subscribe({
      next: () => {
        this.cookbookEntryId.set(null);
        this.cookbookSaving.set(false);
      },
      error: () => {
        this.cookbookSaving.set(false);
        this.cookbookError.set("Couldn't remove that recipe from your cookbook. Try again.");
      },
    });
  }

  protected print(): void {
    window.print();
  }

  private loadCookbookMembership(): void {
    const recipeId = this.recipeId;
    if (recipeId === null) {
      return;
    }

    this.cookbookLoading.set(true);
    this.cookbookError.set(null);

    this.cookbooks.getApiCookbooks('application/json').subscribe({
      next: (cookbooks) => {
        const family = cookbooks.find((item) => item.name === 'Family Cookbook') ?? cookbooks[0];
        const cookbookId = this.numericId(family?.id);
        if (cookbookId === null) {
          this.cookbookLoading.set(false);
          this.cookbookError.set("Couldn't find your family cookbook.");
          return;
        }

        this.cookbookId.set(cookbookId);
        this.loadCookbookEntries(cookbookId, recipeId);
      },
      error: () => {
        this.cookbookLoading.set(false);
        this.cookbookError.set("Couldn't load your family cookbook.");
      },
    });
  }

  private loadCookbookEntries(cookbookId: number, recipeId: number): void {
    this.cookbooks.getApiCookbooksCookbookIdEntries(cookbookId, 'application/json').subscribe({
      next: (entries) => {
        const match = entries.find((entry) => this.numericId(entry.sourceRecipeId) === recipeId);
        this.cookbookEntryId.set(this.numericId(match?.id));
        this.cookbookLoading.set(false);
      },
      error: () => {
        this.cookbookLoading.set(false);
        this.cookbookError.set("Couldn't load your family cookbook.");
      },
    });
  }

  private numericId(value: number | string | null | undefined): number | null {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const id = Number(value);
    return Number.isInteger(id) && id > 0 ? id : null;
  }
}
