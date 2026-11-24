import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { MainLayout } from './layouts/main-layout/main-layout';
import { Recipes } from './pages/recipes/recipes';
import { RecipePaper } from './pages/recipe-paper/recipe-paper';
import { Cookbook } from './pages/cookbook/cookbook';
import { CookbookEntry } from './pages/cookbook-entry/cookbook-entry';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      { path: '', component: Home },
      { path: 'about', component: Home },
      { path: 'recipes', component: Recipes },
      { path: 'recipes/:id', component: RecipePaper },
      { path: 'cookbook', component: Cookbook },
      { path: 'cookbook/:cookbookId/entries/:entryId', component: CookbookEntry },
    ],
  },
  { path: 'login', component: Home }, // no navbar
];
