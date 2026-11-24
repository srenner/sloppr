import { Component, computed, inject } from '@angular/core';
import { LucideMonitor, LucideMoon, LucideSun } from '@lucide/angular';
import { ThemeService } from '../../core/theme.service';

@Component({
  selector: 'app-theme-toggle',
  imports: [LucideMonitor, LucideMoon, LucideSun],
  templateUrl: './theme-toggle.html',
  styleUrl: './theme-toggle.css',
})
export class ThemeToggle {
  private readonly theme = inject(ThemeService);

  protected readonly mode = this.theme.mode;
  protected readonly label = computed(() => {
    switch (this.mode()) {
      case 'light':
        return 'Theme: light. Switch to dark';
      case 'dark':
        return 'Theme: dark. Switch to system';
      default:
        return 'Theme: system. Switch to light';
    }
  });

  protected cycle(): void {
    this.theme.cycle();
  }
}
