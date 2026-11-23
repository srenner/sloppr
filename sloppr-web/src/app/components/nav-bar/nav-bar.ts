import { Component, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideSettings } from '@lucide/angular';

@Component({
  selector: 'app-nav-bar',
  imports: [LucideSettings, RouterLink],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.css',
})
export class NavBar {
  readonly settingsClick = output<void>();
}
