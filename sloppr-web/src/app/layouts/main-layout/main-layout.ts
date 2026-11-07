import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from '../../components/nav-bar/nav-bar';
import { SettingsModal } from '../../components/settings-modal/settings-modal';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
  imports: [RouterOutlet, NavBar, SettingsModal],
})
export class MainLayout {}
