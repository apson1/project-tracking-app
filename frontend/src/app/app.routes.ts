import { Routes } from '@angular/router';
import { Dashboard } from './components/dashboard/dashboard';
import { Projects } from './components/projects/projects';
import { ProjectDetail } from './components/project-detail/project-detail';
import { Participants } from './components/participants/participants';
import { OutputEntry } from './components/output-entry/output-entry';
import { ImportAdmin } from './components/import-admin/import-admin';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: Dashboard },
  { path: 'projects', component: Projects },
  { path: 'projects/:id', component: ProjectDetail },
  { path: 'participants', component: Participants },
  { path: 'output-entry', component: OutputEntry },
  { path: 'import-admin', component: ImportAdmin },
  { path: '**', redirectTo: 'dashboard' }
];
