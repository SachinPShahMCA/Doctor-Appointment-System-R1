import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { tap, catchError } from 'rxjs/operators';
import { of, throwError } from 'rxjs';

export interface User {
  id: string;
  email: string;
  role: string;
  tenantId: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  fullName: string;
  role: string;
  tenantId: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'docapp_jwt';
  private readonly TENANT_KEY = 'docapp_tenant';
  
  // Reactive state using Angular Signals
  currentUser = signal<User | null>(null);
  isAuthenticated = signal<boolean>(false);
  currentTenant = signal<string>('demo'); // Default to demo tenant

  constructor(private http: HttpClient) {
    this.restoreSession();
  }

  // Set the tenant ID explicitly before login
  setTenant(tenantId: string) {
    this.currentTenant.set(tenantId);
    localStorage.setItem(this.TENANT_KEY, tenantId);
  }

  getTenant(): string {
    return this.currentTenant();
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  login(email: string, password: string) {
    // We rely on the interceptor to attach X-Tenant-ID header globally based on currentTenant signal
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/Auth/login`, { email, password })
      .pipe(
        tap(res => this.handleAuthSuccess(res)),
        catchError(err => throwError(() => err))
      );
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  private handleAuthSuccess(res: LoginResponse) {
    localStorage.setItem(this.TOKEN_KEY, res.token);
    this.currentUser.set({
      id: '', // Extract from JWT ideally, or rely on /me
      email: res.email,
      role: res.role,
      tenantId: res.tenantId
    });
    this.isAuthenticated.set(true);
  }

  private restoreSession() {
    const token = this.getToken();
    const tenant = localStorage.getItem(this.TENANT_KEY) || 'demo';
    this.currentTenant.set(tenant);

    if (token) {
      // Very basic JWT presence check (should also check expiry but simplicity wins for now)
      this.isAuthenticated.set(true);
      // Optional: Call GET /api/Auth/me to hydrate the full User object
    }
  }
}
