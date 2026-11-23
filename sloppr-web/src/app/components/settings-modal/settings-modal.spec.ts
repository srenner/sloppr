import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SettingsModal } from './settings-modal';

describe('SettingsModal', () => {
  let component: SettingsModal;
  let fixture: ComponentFixture<SettingsModal>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsModal],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsModal);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    http.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should open the dialog', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    const showModal = vi.fn();
    dialog.showModal = showModal;

    component.open();
    fixture.detectChanges();

    http.expectOne('/api/AiProviders').flush([]);

    expect(showModal).toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Ollama provider');
  });

  it('should switch to the models tab', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.showModal = vi.fn();

    component.open();
    fixture.detectChanges();

    http.expectOne('/api/AiProviders').flush([]);
    fixture.detectChanges();

    const tabs = fixture.nativeElement.querySelectorAll(
      'button[role="tab"]',
    ) as NodeListOf<HTMLButtonElement>;
    tabs[1].click();
    fixture.detectChanges();

    http.expectOne('/api/AiProviders').flush([]);
    http.expectOne('/api/AiModels').flush([]);
    http.expectOne('/api/AiModels/inactive').flush([]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Discover models');
  });

  it('should switch to the capabilities tab', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.showModal = vi.fn();

    component.open();
    fixture.detectChanges();

    http.expectOne('/api/AiProviders').flush([]);
    fixture.detectChanges();

    const tabs = fixture.nativeElement.querySelectorAll(
      'button[role="tab"]',
    ) as NodeListOf<HTMLButtonElement>;
    tabs[2].click();
    fixture.detectChanges();

    http.expectOne('/api/AiModels').flush([]);
    http.expectOne('/api/AiCapabilitySettings').flush([]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No models are in use yet.');
  });

  it('should close the dialog from the close button', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    const close = vi.fn();
    dialog.close = close;

    const closeButton = fixture.nativeElement.querySelector(
      'button[aria-label="Close settings"]',
    ) as HTMLButtonElement;
    expect(closeButton).toBeTruthy();

    closeButton.click();

    expect(close).toHaveBeenCalled();
  });
});
