import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { 
    path: 'admin', 
    loadComponent: () => import('./features/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [authGuard] 
  },
  { 
    path: 'doctor', 
    loadComponent: () => import('./features/doctor/doctor-dashboard.component').then(m => m.DoctorDashboardComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'patient', 
    loadComponent: () => import('./features/patient/patient-dashboard.component').then(m => m.PatientDashboardComponent),
    canActivate: [authGuard]
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];
