import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
  private readonly route = inject(ActivatedRoute);

  protected readonly markdown = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      this.error.set('Recipe not found.');
      return;
    }

    this.recipes.getApiRecipesIdMarkdown(id).subscribe({
      next: (markdown) => {
        this.markdown.set(markdown);
        this.loading.set(false);
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
}
