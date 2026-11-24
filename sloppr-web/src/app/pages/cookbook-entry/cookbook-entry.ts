import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CookbooksService } from '../../api/cookbooks/cookbooks.service';
import { RecipesService } from '../../api/recipes/recipes.service';
import { MarkdownView } from '../../components/markdown-view/markdown-view';

@Component({
  selector: 'app-cookbook-entry',
  imports: [MarkdownView, RouterLink],
  templateUrl: './cookbook-entry.html',
  styleUrl: './cookbook-entry.css',
})
export class CookbookEntry {
  private readonly cookbooks = inject(CookbooksService);
  private readonly recipes = inject(RecipesService);
  private readonly route = inject(ActivatedRoute);

  protected readonly markdown = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    const cookbookId = this.numericId(this.route.snapshot.paramMap.get('cookbookId'));
    const entryId = this.numericId(this.route.snapshot.paramMap.get('entryId'));
    if (cookbookId === null || entryId === null) {
      this.loading.set(false);
      this.error.set('Recipe not found.');
      return;
    }

    this.cookbooks
      .getApiCookbooksCookbookIdEntriesEntryId(cookbookId, entryId, 'application/json')
      .subscribe({
        next: (entry) => {
          this.recipes.postApiRecipesMarkdown({ content: entry.content }).subscribe({
            next: (markdown) => {
              this.markdown.set(markdown);
              this.loading.set(false);
            },
            error: () => {
              this.loading.set(false);
              this.error.set("Couldn't render that recipe.");
            },
          });
        },
        error: () => {
          this.loading.set(false);
          this.error.set("Couldn't load that recipe.");
        },
      });
  }

  protected print(): void {
    window.print();
  }

  private numericId(value: string | null): number | null {
    if (value === null || value === '') {
      return null;
    }

    const id = Number(value);
    return Number.isInteger(id) && id > 0 ? id : null;
  }
}
