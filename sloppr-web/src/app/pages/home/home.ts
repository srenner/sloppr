import { Component, computed, signal } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected readonly prompt = signal('');
  protected readonly lastPrompt = signal<string | null>(null);
  protected readonly canSend = computed(() => this.prompt().trim().length > 0);

  protected onInput(event: Event): void {
    this.prompt.set((event.target as HTMLTextAreaElement).value);
  }

  protected send(): void {
    const text = this.prompt().trim();
    if (!text) {
      return;
    }

    // Stub for now; session creation lands in a later iteration.
    this.lastPrompt.set(text);
    this.prompt.set('');
  }
}
