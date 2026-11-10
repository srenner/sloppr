import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SettingsModal } from './settings-modal';

describe('SettingsModal', () => {
  let component: SettingsModal;
  let fixture: ComponentFixture<SettingsModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsModal],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should open the dialog', () => {
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    const showModal = vi.fn();
    dialog.showModal = showModal;

    component.open();

    expect(showModal).toHaveBeenCalled();
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
