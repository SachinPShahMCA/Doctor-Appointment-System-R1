import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container" style="padding-top: var(--space-8)">
      <div class="glass-panel animate-fade-in">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-gradient">Patient Portal</h1>
            <p class="subtitle">Manage your health and upcoming visits.</p>
          </div>
          <button class="btn btn-primary" (click)="logout()">Sign Out</button>
        </div>
        
        <div class="mt-8 flex gap-6" style="margin-top: var(--space-8)">
          <div class="glass-panel" style="flex: 1">
            <h3>My Appointments</h3>
            <p class="text-muted">No upcoming appointments.</p>
            <button class="btn btn-primary" style="margin-top: var(--space-4)">Book New Appointment</button>
          </div>
          <div class="glass-panel" style="flex: 1">
            <h3>My Details</h3>
            <p class="text-muted">View or update your medical records and preferences.</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PatientDashboardComponent {
  constructor(private auth: AuthService) {}

  logout() {
    this.auth.logout();
    window.location.reload();
  }
}
