import { Routes } from '@angular/router';
import { Home } from './pages/home/home'
import { MainLayout } from './layouts/main-layout/main-layout';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      { path: '', component: Home },
      { path: 'about', component: Home },
    ]
  },
  { path: 'login', component: Home }, // no navbar
];