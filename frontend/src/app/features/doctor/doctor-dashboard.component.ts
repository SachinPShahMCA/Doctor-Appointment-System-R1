import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container" style="padding-top: var(--space-8)">
      <div class="glass-panel animate-fade-in">
        <h1 class="text-gradient">Doctor Dashboard</h1>
        <p class="subtitle">Welcome back! Here is your schedule for today.</p>
        
        <div class="mt-8 flex gap-6" style="margin-top: var(--space-8)">
          <div class="glass-panel" style="flex: 1">
            <h3>Upcoming Appointments</h3>
            <p class="text-muted">No appointments found.</p>
          </div>
          <div class="glass-panel" style="flex: 1">
            <h3>Availability Settings</h3>
            <button class="btn btn-outline">Manage Slots</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DoctorDashboardComponent {}
