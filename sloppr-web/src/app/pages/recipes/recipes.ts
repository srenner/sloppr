import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  afterNextRender,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import type { RecipePageDTO, RecipeSummaryDTO } from '../../api/model';
import { RecipesService } from '../../api/recipes/recipes.service';
import { RecipeCard } from '../../components/recipe-card/recipe-card';

const PAGE_SIZE = 24;

@Component({
  selector: 'app-recipes',
  imports: [RecipeCard],
  templateUrl: './recipes.html',
  styleUrl: './recipes.css',
})
export class Recipes implements OnInit, OnDestroy {
  private readonly recipes = inject(RecipesService);
  private readonly sentinel = viewChild<ElementRef<HTMLElement>>('sentinel');

  protected readonly items = signal<RecipeSummaryDTO[]>([]);
  protected readonly total = signal(0);
  protected readonly loading = signal(false);
  protected readonly done = signal(false);
  protected readonly error = signal<string | null>(null);

  private page = 0;
  private observer: IntersectionObserver | null = null;

  constructor() {
    afterNextRender(() => this.observeSentinel());
  }

  ngOnInit(): void {
    this.loadNext();
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  protected loadNext(): void {
    if (this.loading() || this.done()) {
      return;
    }

    const nextPage = this.page + 1;
    this.loading.set(true);
    this.error.set(null);

    this.recipes
      .getApiRecipesPaged('application/json', { page: nextPage, pageSize: PAGE_SIZE })
      .subscribe({
        next: (result) => {
          this.page = nextPage;
          this.append(result);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.error.set("Couldn't load recipes. Check the API is running, then try again.");
        },
      });
  }

  private append(result: RecipePageDTO): void {
    const known = new Set(this.items().map((item) => String(item.id)));
    const fresh = (result.items ?? []).filter((item) => !known.has(String(item.id)));
    const next = [...this.items(), ...fresh];
    const total = Number(result.total ?? 0);

    this.items.set(next);
    this.total.set(total);
    this.done.set(next.length >= total || fresh.length === 0);
  }

  private observeSentinel(): void {
    const element = this.sentinel()?.nativeElement;
    if (!element || typeof IntersectionObserver === 'undefined') {
      return;
    }

    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          this.loadNext();
        }
      },
      { rootMargin: '300px 0px' },
    );
    this.observer.observe(element);
  }
}
