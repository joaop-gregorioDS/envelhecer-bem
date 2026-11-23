import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  nome: string;
}

const TOKEN_KEY = 'eb_token';
const NOME_KEY = 'eb_nome';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _token = signal<string | null>(this.lerStorage(TOKEN_KEY));
  private readonly _nome = signal<string | null>(this.lerStorage(NOME_KEY));

  readonly autenticado = computed(() => !!this._token());
  readonly nome = this._nome.asReadonly();

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((resp) => this.salvarSessao(resp))
    );
  }

  logout(): void {
    this._token.set(null);
    this._nome.set(null);
    this.escreverStorage(TOKEN_KEY, null);
    this.escreverStorage(NOME_KEY, null);
  }

  get token(): string | null {
    return this._token();
  }

  private salvarSessao(resp: LoginResponse): void {
    this._token.set(resp.token);
    this._nome.set(resp.nome);
    this.escreverStorage(TOKEN_KEY, resp.token);
    this.escreverStorage(NOME_KEY, resp.nome);
  }

  // localStorage pode estar indisponível (modo privado, bloqueio de cookies)
  private lerStorage(chave: string): string | null {
    try {
      return localStorage.getItem(chave);
    } catch {
      return null;
    }
  }

  private escreverStorage(chave: string, valor: string | null): void {
    try {
      if (valor === null) localStorage.removeItem(chave);
      else localStorage.setItem(chave, valor);
    } catch {
      // ignora: sessão continua apenas em memória
    }
  }
}
