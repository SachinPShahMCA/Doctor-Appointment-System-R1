import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="login-container flex items-center justify-center">
      <div class="glass-panel login-card animate-fade-in">
        <div class="header">
          <h1 class="text-gradient">DocApp</h1>
          <p class="subtitle">Sign in to your account</p>
        </div>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          
          <div class="form-group">
            <label class="form-label" for="tenant">Clinic ID (Tenant)</label>
            <input type="text" id="tenant" class="form-control" formControlName="tenant" placeholder="e.g. demo" />
          </div>

          <div class="form-group">
            <label class="form-label" for="email">Email address</label>
            <input type="email" id="email" class="form-control" formControlName="email" placeholder="admin@demo-clinic.com" />
            <div *ngIf="f['email'].touched && f['email'].invalid" class="error-text">
              Valid email is required
            </div>
          </div>

          <div class="form-group">
            <label class="form-label" for="password">Password</label>
            <input type="password" id="password" class="form-control" formControlName="password" placeholder="••••••••" />
          </div>

          <div *ngIf="errorMsg" class="error-banner">
            {{ errorMsg }}
          </div>

          <button type="submit" class="btn btn-primary w-full shadow-hover" [disabled]="loginForm.invalid || loading">
            <span *ngIf="!loading">Sign In</span>
            <span *ngIf="loading">Signing in...</span>
          </button>

          <div class="demo-hints">
            <small>Demo <strong>Doctor</strong>: dr.sarah&#64;demo-clinic.com / Doctor&#64;123</small><br>
            <small>Demo <strong>Patient</strong>: rahul&#64;example.com / Patient&#64;123</small>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      background: var(--bg-main) url('https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?auto=format&fit=crop&q=80&w=2000') center/cover no-repeat;
      position: relative;
    }

    .login-container::before {
      content: '';
      position: absolute;
      inset: 0;
      background: linear-gradient(to bottom right, rgba(15, 23, 42, 0.7), rgba(79, 70, 229, 0.6));
      backdrop-filter: blur(4px);
      -webkit-backdrop-filter: blur(4px);
    }

    .login-card {
      width: 100%;
      max-width: 420px;
      position: relative;
      background: var(--bg-surface); /* override glass for better contrast if needed, or keep glass */
      z-index: 10;
    }

    .header {
      text-align: center;
      margin-bottom: var(--space-6);
    }
    
    .header h1 {
      font-size: 2.5rem;
      margin-bottom: 0px;
      letter-spacing: -1px;
    }
    
    .subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
    }

    .w-full { width: 100%; }
    
    .btn {
      padding: 0.75rem;
      font-size: 1rem;
      font-weight: 600;
      letter-spacing: 0.5px;
    }

    .shadow-hover:hover {
      box-shadow: 0 10px 15px -3px rgba(79, 70, 229, 0.4);
    }

    .error-text {
      color: var(--accent);
      font-size: 0.75rem;
      margin-top: 4px;
    }

    .error-banner {
      background-color: rgba(244, 63, 94, 0.1);
      color: var(--accent);
      padding: var(--space-3);
      border-radius: var(--radius-md);
      font-size: 0.875rem;
      margin-bottom: var(--space-4);
      text-align: center;
      border: 1px solid rgba(244, 63, 94, 0.3);
    }

    .demo-hints {
      margin-top: var(--space-6);
      padding-top: var(--space-4);
      border-top: 1px solid var(--border-light);
      color: var(--text-secondary);
      line-height: 1.6;
    }
  `]
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  errorMsg = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      tenant: ['demo', Validators.required],
      email: ['admin@demo-clinic.com', [Validators.required, Validators.email]],
      password: ['Admin@123', Validators.required]
    });
  }

  get f() { return this.loginForm.controls; }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.errorMsg = '';
    const val = this.loginForm.value;

    this.authService.setTenant(val.tenant);
    
    this.authService.login(val.email, val.password).subscribe({
      next: (res) => {
        // Redirect based on role
        if (res.role === 'Admin') this.router.navigate(['/admin']);
        else if (res.role === 'Doctor') this.router.navigate(['/doctor']);
        else this.router.navigate(['/patient']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = err?.error?.message || 'Invalid credentials or server unavailable.';
      }
    });
  }
}
