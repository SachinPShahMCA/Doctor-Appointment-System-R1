import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container" style="padding-top: var(--space-8)">
      <div class="glass-panel animate-fade-in">
        <h1 class="text-gradient">Admin Dashboard</h1>
        <p class="subtitle">Tenant Management & System Configuration</p>
        
        <div class="mt-8" style="margin-top: var(--space-8)">
          <div class="glass-panel">
            <h3>Recent Tenants</h3>
            <p class="text-muted">No tenants to display yet.</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AdminDashboardComponent {}
