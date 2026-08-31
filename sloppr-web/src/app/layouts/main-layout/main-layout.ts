import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
// import { NavBar } from '../../components/nav-bar/nav-bar';

@Component({
  selector: 'app-main-layout',
  // imports: [RouterOutlet, NavBar],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
  imports: [RouterOutlet],
})
export class MainLayout {}
