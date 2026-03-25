import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Attaches the JWT Bearer token and X-Tenant-ID header to every outgoing request.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  const tenantId = authService.getTenant();

  let headers = req.headers;

  if (tenantId) {
    headers = headers.set('X-Tenant-ID', tenantId);
  }

  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }

  const clonedReq = req.clone({ headers });
  
  return next(clonedReq);
};
