import { Component, ElementRef, viewChild } from '@angular/core';

@Component({
  selector: 'app-settings-modal',
  templateUrl: './settings-modal.html',
  styleUrl: './settings-modal.css',
})
export class SettingsModal {
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  open(): void {
    this.dialog().nativeElement.showModal();
  }

  protected close(): void {
    this.dialog().nativeElement.close();
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialog().nativeElement) {
      this.close();
    }
  }
}
