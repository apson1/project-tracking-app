import { Component, ChangeDetectorRef } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService, User } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  title = 'Project Tracking System';
  showDropdown = false;

  constructor(public authService: AuthService, private cdr: ChangeDetectorRef) {}

  toggleDropdown(): void {
    this.showDropdown = !this.showDropdown;
    this.cdr.detectChanges();
  }

  selectUser(user: User): void {
    this.authService.setCurrentUser(user);
    this.showDropdown = false;
    this.cdr.detectChanges();
    // Force reload/re-render of current view to update authorization rules
    window.location.reload();
  }
}
