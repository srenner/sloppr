import { Component, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AiCapabilitySettingsService } from '../../api/ai-capability-settings/ai-capability-settings.service';
import { AiModelsService } from '../../api/ai-models/ai-models.service';
import { AiCapabilityType } from '../../api/model';
import type { AiCapabilitySettingDTO, AiModelDTO } from '../../api/model';

type CapabilityForm = {
  capability: AiCapabilityType;
  label: string;
  modelId: string;
  systemPrompt: string;
};

const CAPABILITY_ORDER: AiCapabilityType[] = [
  AiCapabilityType.IngredientExtraction,
  AiCapabilityType.IdeaGeneration,
  AiCapabilityType.RecipeGeneration,
  AiCapabilityType.OcrScanning,
];

const CAPABILITY_LABELS: Record<AiCapabilityType, string> = {
  [AiCapabilityType.IngredientExtraction]: 'Ingredient Extraction',
  [AiCapabilityType.IdeaGeneration]: 'Idea Generation',
  [AiCapabilityType.RecipeGeneration]: 'Recipe Generation',
  [AiCapabilityType.OcrScanning]: 'Recipe Image Scanner',
};

@Component({
  selector: 'app-settings-capabilities',
  templateUrl: './settings-capabilities.html',
  styleUrl: './settings-capabilities.css',
})
export class SettingsCapabilities {
  private readonly modelsService = inject(AiModelsService);
  private readonly settingsService = inject(AiCapabilitySettingsService);

  protected readonly models = signal<AiModelDTO[]>([]);
  protected readonly capabilities = signal<CapabilityForm[]>([]);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected readonly canSave = computed(
    () =>
      this.capabilities().length > 0 &&
      this.capabilities().every((capability) => capability.systemPrompt.trim().length > 0),
  );

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      models: this.modelsService.getApiAiModels('application/json'),
      settings: this.settingsService.getApiAiCapabilitySettings('application/json'),
    }).subscribe({
      next: ({ models, settings }) => {
        this.models.set(models);
        this.applySettings(settings);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(
          "Couldn't load the capability defaults. Check that the API is running.",
        );
      },
    });
  }

  protected onModelChange(capability: AiCapabilityType, event: Event): void {
    const modelId = (event.target as HTMLSelectElement).value;
    this.capabilities.update((items) =>
      items.map((item) => (item.capability === capability ? { ...item, modelId } : item)),
    );
  }

  protected onPromptChange(capability: AiCapabilityType, event: Event): void {
    const systemPrompt = (event.target as HTMLTextAreaElement).value;
    this.capabilities.update((items) =>
      items.map((item) => (item.capability === capability ? { ...item, systemPrompt } : item)),
    );
  }

  protected save(): void {
    if (this.saving() || !this.canSave()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.settingsService
      .putApiAiCapabilitySettings(
        this.capabilities().map((capability) => ({
          capability: capability.capability,
          modelId: this.toId(capability.modelId),
          systemPrompt: capability.systemPrompt.trim(),
        })),
        'application/json',
      )
      .subscribe({
        next: (settings) => {
          this.applySettings(settings);
          this.saving.set(false);
          this.notice.set('Capability defaults saved.');
        },
        error: () => {
          this.saving.set(false);
          this.error.set("Couldn't save the capability defaults.");
        },
      });
  }

  protected stringId(id: number | string | null | undefined): string {
    return id == null ? '' : String(id);
  }

  private applySettings(settings: AiCapabilitySettingDTO[]): void {
    const byCapability = new Map<AiCapabilityType, AiCapabilitySettingDTO>();
    for (const setting of settings) {
      byCapability.set(setting.capability, setting);
    }

    this.capabilities.set(
      CAPABILITY_ORDER.map((capability) => ({
        capability,
        label: CAPABILITY_LABELS[capability],
        modelId: this.activeModelId(byCapability.get(capability)),
        systemPrompt: byCapability.get(capability)?.systemPrompt ?? '',
      })),
    );
  }

  private activeModelId(setting: AiCapabilitySettingDTO | undefined): string {
    const modelId = setting?.modelId;
    if (modelId == null) {
      return '';
    }

    return this.models().some((model) => this.stringId(model.id) === this.stringId(modelId))
      ? this.stringId(modelId)
      : '';
  }

  private toId(value: string): number | null {
    return value === '' ? null : Number(value);
  }
}
