import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'eb-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.autenticado()) {
      <header class="topo">
        <strong class="marca">Envelhecer Bem</strong>
        <nav>
          <a routerLink="/painel" routerLinkActive="ativo">Painel</a>
          <a routerLink="/registro" routerLinkActive="ativo">Registrar</a>
          <a routerLink="/lembretes" routerLinkActive="ativo">Lembretes</a>
          <span class="usuario">{{ auth.nome() }}</span>
          <button class="btn-sair" type="button" (click)="sair()">Sair</button>
        </nav>
      </header>
    }
    <div class="conteudo">
      <router-outlet />
    </div>
  `,
  styles: `
    .topo {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.75rem;
      padding: 0.9rem 1.5rem;
      background: var(--vx-gradient);
      color: #fff;
    }
    .marca { font-size: 1.1rem; }
    nav { display: flex; gap: 1rem; align-items: center; flex-wrap: wrap; }
    nav a { color: #fff; text-decoration: none; opacity: 0.85; font-weight: 500; }
    nav a.ativo, nav a:hover { opacity: 1; text-decoration: underline; }
    .usuario { opacity: 0.85; font-size: 0.9rem; }
    .btn-sair {
      background: rgba(255, 255, 255, 0.2);
      color: #fff;
      border: 1px solid rgba(255, 255, 255, 0.5);
      border-radius: var(--vx-radius);
      padding: 0.35rem 0.8rem;
      cursor: pointer;
      font: inherit;
    }
    .conteudo { padding: 1.5rem; max-width: 1100px; margin: 0 auto; }
  `
})
export class App {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  sair(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
