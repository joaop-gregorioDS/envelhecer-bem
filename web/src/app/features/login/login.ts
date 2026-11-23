import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'eb-login',
  imports: [ReactiveFormsModule],
  template: `
    <main class="login-page">
      <section class="card login-card">
        <h1>Envelhecer Bem</h1>
        <p class="sub">Registre seus hábitos e acompanhe sua evolução.</p>
        <p class="dados-ficticios">Dados fictícios para demonstração: maria&#64;exemplo.com / Demo&#64;123</p>

        @if (erro()) {
          <p class="alerta-erro" role="alert">{{ erro() }}</p>
        }

        <form [formGroup]="form" (ngSubmit)="entrar()">
          <div class="field">
            <label for="email">E-mail</label>
            <input id="email" type="email" formControlName="email" autocomplete="username" />
            @if (form.controls.email.touched && form.controls.email.invalid) {
              <span class="erro">Informe um e-mail válido.</span>
            }
          </div>

          <div class="field">
            <label for="senha">Senha</label>
            <input id="senha" type="password" formControlName="senha" autocomplete="current-password" />
            @if (form.controls.senha.touched && form.controls.senha.invalid) {
              <span class="erro">Informe a senha.</span>
            }
          </div>

          <button class="btn btn-block" type="submit" [disabled]="carregando()">
            {{ carregando() ? 'Entrando...' : 'Entrar' }}
          </button>
        </form>
      </section>
    </main>
  `,
  styles: `
    .login-page {
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: 1rem;
      background: var(--vx-background);
    }
    .login-card {
      width: min(420px, 100%);
      padding: 2rem;
    }
    h1 { margin: 0 0 0.25rem; color: var(--vx-foreground); font-weight: 400; font-size: 2rem; }
    .sub { color: var(--vx-muted); margin: 0 0 1rem; }
    .dados-ficticios {
      font-size: 0.8rem;
      color: var(--vx-muted);
      background: #fff;
      padding: 0.5rem 0.75rem;
      border-radius: var(--vx-radius);
      margin: 0 0 1.25rem;
    }
    .btn-block { width: 100%; }
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required]]
  });

  entrar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigateByUrl('/painel'),
      error: (err: HttpErrorResponse) => {
        this.carregando.set(false);
        this.erro.set(
          err.status === 401 ? 'E-mail ou senha inválidos.' : 'Não foi possível entrar. Tente novamente.'
        );
      }
    });
  }
}
