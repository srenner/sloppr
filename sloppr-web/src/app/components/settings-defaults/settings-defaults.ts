import { Component, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AiModelsService } from '../../api/ai-models/ai-models.service';
import { ApplicationSettingsService } from '../../api/application-settings/application-settings.service';
import type { AiModelDTO } from '../../api/model';

@Component({
  selector: 'app-settings-defaults',
  templateUrl: './settings-defaults.html',
  styleUrl: './settings-defaults.css',
})
export class SettingsDefaults {
  private readonly modelsService = inject(AiModelsService);
  private readonly settingsService = inject(ApplicationSettingsService);

  protected readonly models = signal<AiModelDTO[]>([]);
  protected readonly extractionModelId = signal('');
  protected readonly ideaModelId = signal('');
  protected readonly recipeModelId = signal('');
  protected readonly ocrModelId = signal('');

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected readonly hasSelection = computed(
    () =>
      this.extractionModelId() !== '' ||
      this.ideaModelId() !== '' ||
      this.recipeModelId() !== '' ||
      this.ocrModelId() !== '',
  );

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      models: this.modelsService.getApiAiModels('application/json'),
      settings: this.settingsService.getApiApplicationSettings('application/json'),
    }).subscribe({
      next: ({ models, settings }) => {
        this.models.set(models);
        this.extractionModelId.set(this.stringId(settings.extractionModelId));
        this.ideaModelId.set(this.stringId(settings.ideaModelId));
        this.recipeModelId.set(this.stringId(settings.recipeModelId));
        this.ocrModelId.set(this.stringId(settings.ocrModelId));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set("Couldn't load the model defaults. Check that the API is running.");
      },
    });
  }

  protected onExtractionChange(event: Event): void {
    this.extractionModelId.set((event.target as HTMLSelectElement).value);
  }

  protected onIdeaChange(event: Event): void {
    this.ideaModelId.set((event.target as HTMLSelectElement).value);
  }

  protected onRecipeChange(event: Event): void {
    this.recipeModelId.set((event.target as HTMLSelectElement).value);
  }

  protected onOcrChange(event: Event): void {
    this.ocrModelId.set((event.target as HTMLSelectElement).value);
  }

  protected save(): void {
    if (this.saving() || !this.hasSelection()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.settingsService
      .putApiApplicationSettings(
        {
          extractionModelId: this.toId(this.extractionModelId()),
          ideaModelId: this.toId(this.ideaModelId()),
          recipeModelId: this.toId(this.recipeModelId()),
          ocrModelId: this.toId(this.ocrModelId()),
        },
        'application/json',
      )
      .subscribe({
        next: (settings) => {
          this.extractionModelId.set(this.stringId(settings.extractionModelId));
          this.ideaModelId.set(this.stringId(settings.ideaModelId));
          this.recipeModelId.set(this.stringId(settings.recipeModelId));
          this.ocrModelId.set(this.stringId(settings.ocrModelId));
          this.saving.set(false);
          this.notice.set('Default models saved.');
        },
        error: () => {
          this.saving.set(false);
          this.error.set("Couldn't save the model defaults.");
        },
      });
  }

  protected stringId(id: number | string | null | undefined): string {
    return id == null ? '' : String(id);
  }

  private toId(value: string): number | null {
    return value === '' ? null : Number(value);
  }
}
