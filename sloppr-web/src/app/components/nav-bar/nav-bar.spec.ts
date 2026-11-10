import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavBar } from './nav-bar';

describe('NavBar', () => {
  let component: NavBar;
  let fixture: ComponentFixture<NavBar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavBar],
    }).compileComponents();

    fixture = TestBed.createComponent(NavBar);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
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
