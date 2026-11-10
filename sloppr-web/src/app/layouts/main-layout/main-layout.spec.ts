import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { SettingsModal } from '../../components/settings-modal/settings-modal';
import { MainLayout } from './main-layout';

describe('MainLayout', () => {
  let component: MainLayout;
  let fixture: ComponentFixture<MainLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MainLayout],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(MainLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the app shell', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-nav-bar')).toBeTruthy();
    expect(compiled.querySelector('router-outlet')).toBeTruthy();
    expect(compiled.querySelector('app-settings-modal')).toBeTruthy();
  });

  it('should open settings from the nav bar', () => {
    const settingsModal = fixture.debugElement.query(By.directive(SettingsModal))!
      .componentInstance as SettingsModal;
    const openSpy = vi.spyOn(settingsModal, 'open').mockImplementation(() => undefined);

    const settingsButton = fixture.nativeElement.querySelector(
      'app-nav-bar button[aria-label="Settings"]',
    ) as HTMLButtonElement;
    expect(settingsButton).toBeTruthy();

    settingsButton.click();
    fixture.detectChanges();

    expect(openSpy).toHaveBeenCalled();
  });
});
