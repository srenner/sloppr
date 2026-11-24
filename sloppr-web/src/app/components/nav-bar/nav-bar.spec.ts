import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { NavBar } from './nav-bar';

describe('NavBar', () => {
  let component: NavBar;
  let fixture: ComponentFixture<NavBar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavBar],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(NavBar);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('links to the cookbook', () => {
    const link = fixture.nativeElement.querySelector(
      'a[href="/cookbook"]',
    ) as HTMLAnchorElement;
    expect(link).toBeTruthy();
    expect(link.textContent).toContain('Cookbook');
  });

  it('should emit settingsClick when settings is clicked', () => {
    let emitted = 0;
    component.settingsClick.subscribe(() => emitted++);

    const settingsButton = fixture.nativeElement.querySelector(
      'button[aria-label="Settings"]',
    ) as HTMLButtonElement;
    expect(settingsButton).toBeTruthy();

    settingsButton.click();

    expect(emitted).toBe(1);
  });
});
