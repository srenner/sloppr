import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { AiProvidersService } from '../../api/ai-providers/ai-providers.service';
import type { AiProviderDTO } from '../../api/model';

const OLLAMA_NAME = 'Ollama';
const OLLAMA_BASE_URL = 'http://localhost:11434';

@Component({
  selector: 'app-settings-provider',
  templateUrl: './settings-provider.html',
  styleUrl: './settings-provider.css',
})
export class SettingsProvider {
  private readonly providersService = inject(AiProvidersService);

  protected readonly providers = signal<AiProviderDTO[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  protected readonly checkingId = signal<string | null>(null);

  protected readonly name = signal(OLLAMA_NAME);
  protected readonly baseUrl = signal(OLLAMA_BASE_URL);
  protected readonly canSave = computed(
    () => this.name().trim().length > 0 && this.baseUrl().trim().length > 0 && !this.saving(),
  );

  protected readonly editingId = signal<string | null>(null);
  protected readonly editName = signal('');
  protected readonly editBaseUrl = signal('');
  protected readonly editSaving = signal(false);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.providersService.getApiAiProviders('application/json').subscribe({
      next: (providers) => {
        this.providers.set(providers);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set("Couldn't load providers. Check that the API is running.");
      },
    });
  }

  protected onNameInput(event: Event): void {
    this.name.set((event.target as HTMLInputElement).value);
  }

  protected onBaseUrlInput(event: Event): void {
    this.baseUrl.set((event.target as HTMLInputElement).value);
  }

  protected save(): void {
    if (!this.canSave()) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.providersService
      .postApiAiProviders(
        {
          name: this.name().trim(),
          providerType: 'Ollama',
          baseUrl: this.baseUrl().trim(),
          isActive: true,
        },
        'application/json',
      )
      .subscribe({
        next: (provider) => {
          this.providers.update((providers) => [...providers, provider]);
          this.saving.set(false);
          this.notice.set('Provider added.');
          this.name.set(OLLAMA_NAME);
          this.baseUrl.set(OLLAMA_BASE_URL);
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error.set(this.describeSaveError(error));
        },
      });
  }

  protected recheck(provider: AiProviderDTO): void {
    if (provider.id == null || this.checkingId() !== null) {
      return;
    }

    const id = provider.id;
    this.checkingId.set(String(id));
    this.error.set(null);
    this.notice.set(null);

    this.providersService.postApiAiProvidersIdHealth(id, 'application/json').subscribe({
      next: (health) => {
        this.providers.update((providers) =>
          providers.map((item) => (String(item.id) === String(id) ? { ...item, ...health } : item)),
        );
        this.checkingId.set(null);
      },
      error: () => {
        this.checkingId.set(null);
        this.error.set("Couldn't reach that provider for a health check.");
      },
    });
  }

  protected startEdit(provider: AiProviderDTO): void {
    this.editingId.set(provider.id == null ? null : String(provider.id));
    this.editName.set(provider.name ?? '');
    this.editBaseUrl.set(provider.baseUrl ?? '');
    this.error.set(null);
    this.notice.set(null);
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
  }

  protected onEditNameInput(event: Event): void {
    this.editName.set((event.target as HTMLInputElement).value);
  }

  protected onEditBaseUrlInput(event: Event): void {
    this.editBaseUrl.set((event.target as HTMLInputElement).value);
  }

  protected saveEdit(provider: AiProviderDTO): void {
    if (provider.id == null || this.editSaving()) {
      return;
    }

    const name = this.editName().trim();
    const baseUrl = this.editBaseUrl().trim();
    if (!name || !baseUrl) {
      return;
    }

    this.editSaving.set(true);
    this.error.set(null);
    this.notice.set(null);

    const id = provider.id;
    this.providersService
      .putApiAiProvidersId(id, {
        name,
        providerType: provider.providerType ?? 'Ollama',
        baseUrl,
        isActive: provider.isActive ?? true,
      })
      .subscribe({
        next: () => {
          this.providers.update((providers) =>
            providers.map((item) =>
              String(item.id) === String(id) ? { ...item, name, baseUrl } : item,
            ),
          );
          this.editSaving.set(false);
          this.editingId.set(null);
          this.notice.set('Provider updated. Recheck the connection to refresh its health.');
        },
        error: () => {
          this.editSaving.set(false);
          this.error.set("Couldn't update the provider.");
        },
      });
  }

  protected remove(provider: AiProviderDTO): void {
    if (provider.id == null) {
      return;
    }

    if (
      !confirm(`Remove ${provider.name || 'this provider'}? Models from it will stop being used.`)
    ) {
      return;
    }

    this.error.set(null);
    this.notice.set(null);

    const id = provider.id;
    this.providersService.deleteApiAiProvidersId(id).subscribe({
      next: () => {
        this.providers.update((providers) =>
          providers.filter((item) => String(item.id) !== String(id)),
        );
        this.notice.set('Provider removed.');
      },
      error: () => {
        this.error.set("Couldn't remove the provider.");
      },
    });
  }

  protected stringId(id: number | string | undefined): string {
    return id == null ? '' : String(id);
  }

  private describeSaveError(error: HttpErrorResponse): string {
    const body: unknown = error.error;

    if (body && typeof body === 'object') {
      const provider = body as AiProviderDTO;
      const status = provider.lastHealthStatusCode;
      const detail = provider.lastHealthResponse?.trim();

      const statusText = status != null ? ` (status ${status})` : '';
      const reason = detail ? ` ${detail}` : '';
      return `Couldn't reach Ollama${statusText}.${reason}`;
    }

    if (typeof body === 'string' && body.trim().length > 0) {
      return body.trim();
    }

    return "Couldn't save the provider. Check that Ollama is running and the URL is correct.";
  }
}
