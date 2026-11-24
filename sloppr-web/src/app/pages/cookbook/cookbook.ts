import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CookbooksService } from '../../api/cookbooks/cookbooks.service';
import type { CookbookDTO, CookbookEntrySummaryDTO } from '../../api/model';

@Component({
  selector: 'app-cookbook',
  imports: [RouterLink],
  templateUrl: './cookbook.html',
  styleUrl: './cookbook.css',
})
export class Cookbook implements OnInit {
  private readonly cookbooks = inject(CookbooksService);

  protected readonly cookbook = signal<CookbookDTO | null>(null);
  protected readonly entries = signal<CookbookEntrySummaryDTO[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly removingId = signal<number | null>(null);
  protected readonly removeError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.removeError.set(null);

    this.cookbooks.getApiCookbooks('application/json').subscribe({
      next: (cookbooks) => {
        const family = cookbooks.find((item) => item.name === 'Family Cookbook') ?? cookbooks[0];
        const cookbookId = this.numericId(family?.id);
        if (!family || cookbookId === null) {
          this.loading.set(false);
          this.error.set("Couldn't find your family cookbook.");
          return;
        }

        this.cookbook.set(family);
        this.loadEntries(cookbookId);
      },
      error: () => {
        this.loading.set(false);
        this.error.set("Couldn't load your cookbook. Check the API is running, then try again.");
      },
    });
  }

  protected remove(entry: CookbookEntrySummaryDTO): void {
    const cookbookId = this.numericId(this.cookbook()?.id);
    const entryId = this.numericId(entry.id);
    if (cookbookId === null || entryId === null || this.removingId() !== null) {
      return;
    }

    this.removingId.set(entryId);
    this.removeError.set(null);

    this.cookbooks.deleteApiCookbooksCookbookIdEntriesEntryId(cookbookId, entryId).subscribe({
      next: () => {
        this.entries.update((items) =>
          items.filter((item) => String(item.id) !== String(entry.id)),
        );
        this.removingId.set(null);
      },
      error: () => {
        this.removingId.set(null);
        this.removeError.set("Couldn't remove that recipe from your cookbook. Try again.");
      },
    });
  }

  protected isRemoving(entry: CookbookEntrySummaryDTO): boolean {
    const entryId = this.numericId(entry.id);
    return entryId !== null && this.removingId() === entryId;
  }

  protected entryLink(entry: CookbookEntrySummaryDTO): (string | number)[] | null {
    const cookbookId = this.numericId(entry.cookbookId);
    const entryId = this.numericId(entry.id);
    if (cookbookId === null || entryId === null) {
      return null;
    }

    return ['/cookbook', cookbookId, 'entries', entryId];
  }

  private loadEntries(cookbookId: number): void {
    this.cookbooks.getApiCookbooksCookbookIdEntries(cookbookId, 'application/json').subscribe({
      next: (entries) => {
        this.entries.set(entries);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set("Couldn't load your cookbook entries. Try again.");
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
