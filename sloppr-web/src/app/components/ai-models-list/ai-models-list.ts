import { Component, inject, OnInit } from '@angular/core';
import { AiModelsService } from '../../api/ai-models/ai-models.service';
import { AiModel } from '../../api/model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ai-models-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ai-models-list.html',
  styleUrls: ['./ai-models-list.css']
})
export class AiModelsListComponent implements OnInit {
  private readonly aiModelsService = inject(AiModelsService);
  models: AiModel[] = [];

  ngOnInit() {
    this.aiModelsService.getApiAiModels('application/json').subscribe((data) => {
      if (Array.isArray(data)) {
        this.models = data;
      }
    });
  }
}
