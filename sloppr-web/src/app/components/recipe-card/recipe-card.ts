import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { RecipeSummaryDTO } from '../../api/model';

@Component({
  selector: 'app-recipe-card',
  imports: [RouterLink],
  templateUrl: './recipe-card.html',
  styleUrl: './recipe-card.css',
})
export class RecipeCard {
  readonly recipe = input.required<RecipeSummaryDTO>();

  protected difficultyLabel(difficulty: number | string | null | undefined): string | null {
    switch (Number(difficulty)) {
      case 1:
        return 'Easy';
      case 2:
        return 'Medium';
      case 3:
        return 'Hard';
      default:
        return null;
    }
  }
}
