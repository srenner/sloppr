import { Component, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AiModelsService } from '../../api/ai-models/ai-models.service';
import { AiProvidersService } from '../../api/ai-providers/ai-providers.service';
import type {
  AiModelDTO,
  AiModelMinimalDTO,
  AiModelRequest,
  AiProviderDTO,
} from '../../api/model';

@Component({
  selector: 'app-settings-models',
  templateUrl: './settings-models.html',
  styleUrl: './settings-models.css',
})
export class SettingsModels {
  private readonly modelsService = inject(AiModelsService);
  private readonly providersService = inject(AiProvidersService);

  protected readonly providers = signal<AiProviderDTO[]>([]);
  protected readonly providerId = signal('');
  protected readonly activeModels = signal<AiModelDTO[]>([]);
  protected readonly inactiveModels = signal<AiModelDTO[]>([]);
  protected readonly discovered = signal<AiModelMinimalDTO[]>([]);
  protected readonly selectedIdentifiers = signal<ReadonlySet<string>>(new Set());
  protected readonly selectedCount = computed(() => this.selectedIdentifiers().size);

  protected readonly loading = signal(true);
  protected readonly discovering = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  constructor() {
    this.loadProviders();
    this.loadModels();
  }

  protected loadProviders(): void {
    this.providersService.getApiAiProviders('application/json').subscribe({
      next: (providers) => {
        const ollama = providers.filter((provider) => provider.providerType === 'Ollama');
        this.providers.set(ollama);
        if (!this.providerId() && ollama.length > 0) {
          this.providerId.set(this.stringId(ollama[0].id));
        }
      },
      error: () => {
        this.error.set("Couldn't load providers. Check that the API is running.");
      },
    });
  }

  protected loadModels(): void {
    this.loading.set(true);

    forkJoin({
      active: this.modelsService.getApiAiModels('application/json'),
      inactive: this.modelsService.getApiAiModelsInactive('application/json'),
    }).subscribe({
      next: ({ active, inactive }) => {
        this.activeModels.set(active);
        this.inactiveModels.set(inactive);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set("Couldn't load models. Check that the API is running.");
      },
    });
  }

  protected onProviderChange(event: Event): void {
    this.providerId.set((event.target as HTMLSelectElement).value);
    this.discovered.set([]);
    this.selectedIdentifiers.set(new Set());
    this.error.set(null);
    this.notice.set(null);
  }

  protected discover(): void {
    const providerId = this.providerId();
    if (!providerId || this.discovering()) {
      if (!providerId) {
        this.error.set('Add an Ollama provider before discovering models.');
      }
      return;
    }

    this.discovering.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.providersService
      .getApiAiProvidersIdDiscoverModels(providerId, 'application/json')
      .subscribe({
        next: (models) => {
          this.discovering.set(false);
          this.discovered.set(models);
          this.selectedIdentifiers.set(new Set());
          if (models.length === 0) {
            this.notice.set('No new models found. Everything installed in Ollama is already configured.');
          }
        },
        error: () => {
          this.discovering.set(false);
          this.error.set("Couldn't discover models. Check that Ollama is running.");
        },
      });
  }

  protected isSelected(identifier: string): boolean {
    return this.selectedIdentifiers().has(identifier);
  }

  protected toggleDiscovered(identifier: string): void {
    const next = new Set(this.selectedIdentifiers());
    if (next.has(identifier)) {
      next.delete(identifier);
    } else {
      next.add(identifier);
    }
    this.selectedIdentifiers.set(next);
  }

  protected addSelected(): void {
    const providerId = this.providerId();
    const selected = this.discovered().filter((model) =>
      this.selectedIdentifiers().has(model.identifier),
    );
    if (!providerId || selected.length === 0 || this.saving()) {
      return;
    }

    const requests: AiModelRequest[] = selected.map((model) => ({
      name: model.name,
      identifier: model.identifier,
      aiProviderId: Number(providerId),
      contextWindow: model.contextWindow ?? null,
      capabilities: model.capabilities ?? null,
      isActive: true,
    }));

    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.modelsService.postApiAiModelsBatch(requests, 'application/json').subscribe({
      next: () => {
        this.saving.set(false);
        this.discovered.update((models) =>
          models.filter((model) => !this.selectedIdentifiers().has(model.identifier)),
        );
        this.selectedIdentifiers.set(new Set());
        this.notice.set('Models added and ready to use.');
        this.loadModels();
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Couldn't add the selected models.");
      },
    });
  }

  protected stopUsing(model: AiModelDTO): void {
    this.setActive(model, false);
  }

  protected use(model: AiModelDTO): void {
    this.setActive(model, true);
  }

  protected stringId(id: number | string | undefined): string {
    return id == null ? '' : String(id);
  }

  private setActive(model: AiModelDTO, isActive: boolean): void {
    if (model.id == null || this.saving()) {
      return;
    }

    const aiProviderId = model.aiProviderId ?? model.aiProvider?.id;
    if (aiProviderId == null) {
      this.error.set('That model has no provider, so it cannot be updated.');
      return;
    }

    const request: AiModelRequest = {
      name: model.name,
      identifier: model.identifier,
      aiProviderId,
      contextWindow: model.contextWindow ?? null,
      capabilities: model.capabilities ?? null,
      isActive,
    };

    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.modelsService.putApiAiModelsId(model.id, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.notice.set(isActive ? 'Model is now in use.' : 'Model is no longer in use.');
        this.loadModels();
      },
      error: () => {
        this.saving.set(false);
        this.error.set("Couldn't update that model.");
      },
    });
  }
}
