import { Component, ElementRef, signal, viewChild } from '@angular/core';
import { SettingsDefaults } from '../settings-defaults/settings-defaults';
import { SettingsModels } from '../settings-models/settings-models';
import { SettingsProvider } from '../settings-provider/settings-provider';

type SettingsTab = 'provider' | 'models' | 'defaults';

@Component({
  selector: 'app-settings-modal',
  imports: [SettingsProvider, SettingsModels, SettingsDefaults],
  templateUrl: './settings-modal.html',
  styleUrl: './settings-modal.css',
})
export class SettingsModal {
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  protected readonly isOpen = signal(false);
  protected readonly tab = signal<SettingsTab>('provider');

  open(): void {
    this.tab.set('provider');
    this.isOpen.set(true);
    this.dialog().nativeElement.showModal();
  }

  protected close(): void {
    this.dialog().nativeElement.close();
  }

  protected onClose(): void {
    this.isOpen.set(false);
  }

  protected selectTab(tab: SettingsTab): void {
    this.tab.set(tab);
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialog().nativeElement) {
      this.close();
    }
  }
}
