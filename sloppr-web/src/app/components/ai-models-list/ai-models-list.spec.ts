import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AiModelsList } from './ai-models-list';

describe('AiModelsList', () => {
  let component: AiModelsList;
  let fixture: ComponentFixture<AiModelsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiModelsList],
    }).compileComponents();

    fixture = TestBed.createComponent(AiModelsList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
