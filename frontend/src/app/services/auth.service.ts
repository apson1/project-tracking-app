import { Injectable, signal } from '@angular/core';

export interface User {
  name: string;
  role: 'admin' | 'reporter' | 'viewer';
  roleName: string;
  initials: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly defaultUsers: User[] = [
    { name: 'Admin User', role: 'admin', roleName: 'System Administrator', initials: 'AU' },
    { name: 'Jane Doe', role: 'reporter', roleName: 'Reporting Unit', initials: 'JD' },
    { name: 'Guest User', role: 'viewer', roleName: 'Guest Viewer', initials: 'GU' }
  ];

  private usersSignal = signal<User[]>([]);
  private currentUserSignal = signal<User>(this.defaultUsers[0]);

  readonly currentUser = this.currentUserSignal.asReadonly();

  get users(): User[] {
    return this.usersSignal();
  }

  constructor() {
    this.loadUsers();
    
    const saved = localStorage.getItem('auth_user');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        const match = this.usersSignal().find(u => u.name === parsed.name && u.role === parsed.role);
        if (match) {
          this.currentUserSignal.set(match);
        } else if (this.usersSignal().length > 0) {
          this.currentUserSignal.set(this.usersSignal()[0]);
        }
      } catch (e) {
        console.error('Failed to parse saved user', e);
      }
    } else if (this.usersSignal().length > 0) {
      this.currentUserSignal.set(this.usersSignal()[0]);
    }
  }

  private loadUsers(): void {
    const savedUsers = localStorage.getItem('auth_users_list');
    if (savedUsers) {
      try {
        const parsed = JSON.parse(savedUsers);
        if (Array.isArray(parsed) && parsed.length > 0) {
          this.usersSignal.set(parsed);
          return;
        }
      } catch (e) {
        console.error('Failed to parse saved users list', e);
      }
    }
    this.usersSignal.set(this.defaultUsers);
    localStorage.setItem('auth_users_list', JSON.stringify(this.defaultUsers));
  }

  setCurrentUser(user: User): void {
    this.currentUserSignal.set(user);
    localStorage.setItem('auth_user', JSON.stringify(user));
  }

  addUser(user: User): void {
    const list = [...this.usersSignal(), user];
    this.usersSignal.set(list);
    localStorage.setItem('auth_users_list', JSON.stringify(list));
  }

  deleteUser(name: string): void {
    const list = this.usersSignal().filter(u => u.name !== name);
    if (list.length === 0) return;
    
    this.usersSignal.set(list);
    localStorage.setItem('auth_users_list', JSON.stringify(list));

    // If we deleted the active user, select the first available one
    if (this.currentUser().name === name) {
      this.setCurrentUser(list[0]);
    }
  }

  isAdmin(): boolean {
    return this.currentUser().role === 'admin';
  }

  isReporterOrAdmin(): boolean {
    const role = this.currentUser().role;
    return role === 'admin' || role === 'reporter';
  }

  hasWriteAccess(): boolean {
    return this.isReporterOrAdmin();
  }

  hasAdminAccess(): boolean {
    return this.isAdmin();
  }
}
