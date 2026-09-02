import { Component } from '@angular/core';
import { AiModelsListComponent } from "../../components/ai-models-list/ai-models-list";

@Component({
  selector: 'app-home',
  imports: [AiModelsListComponent],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {}
