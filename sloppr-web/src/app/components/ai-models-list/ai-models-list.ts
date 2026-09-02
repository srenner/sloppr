import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
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
export class AiModelsListComponent {
  private readonly aiModelsService = inject(AiModelsService);

  models = toSignal(
    this.aiModelsService.getApiAiModels('application/json'),
    { initialValue: [] as AiModel[] }
  );
}
