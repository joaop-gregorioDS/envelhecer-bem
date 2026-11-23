import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // Só anexa o token às chamadas para a própria API
  const ehApi = req.url.startsWith(environment.apiUrl);
  const token = auth.token;

  const reqComToken = ehApi && token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(reqComToken).pipe(
    catchError((erro: HttpErrorResponse) => {
      // Token expirado ou inválido: encerra a sessão e manda para o login
      if (erro.status === 401 && ehApi && !req.url.endsWith('/auth/login')) {
        auth.logout();
        router.navigate(['/login']);
      }
      return throwError(() => erro);
    })
  );
};
