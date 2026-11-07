import { Component, output } from '@angular/core';
import { LucideSettings } from '@lucide/angular';

@Component({
  selector: 'app-nav-bar',
  imports: [LucideSettings],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css',
})
export class NavBar {
  readonly settingsClick = output<void>();
}
