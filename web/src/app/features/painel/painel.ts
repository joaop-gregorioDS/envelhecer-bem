import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';
import { RegistroDiario, ResumoPainel } from '../../core/models/models';

@Component({
  selector: 'eb-painel',
  imports: [DatePipe, DecimalPipe, RouterLink],
  template: `
    <section class="painel">
      <div class="cabecalho">
        <h1>Seu painel</h1>
        <a class="btn" routerLink="/registro">+ Registrar hoje</a>
      </div>

      @if (erro()) {
        <p class="alerta-erro">{{ erro() }}</p>
      }

      @if (resumo(); as r) {
        <div class="indicadores">
          <article class="card indicador">
            <span>Sequência atual</span>
            <strong>{{ r.sequenciaAtual }} dias</strong>
          </article>
          <article class="card indicador">
            <span>Sono médio</span>
            <strong>{{ r.mediaSonoHoras | number: '1.1-1' }} h</strong>
          </article>
          <article class="card indicador">
            <span>Passos médios</span>
            <strong>{{ r.mediaPassos | number }}</strong>
          </article>
          <article class="card indicador">
            <span>Água média</span>
            <strong>{{ r.mediaCoposDeAgua | number: '1.0-1' }} copos</strong>
          </article>
          <article class="card indicador">
            <span>Humor médio</span>
            <strong>{{ r.mediaHumor | number: '1.1-1' }}/5</strong>
          </article>
        </div>
      }

      <article class="card grafico">
        <h2>Passos nos últimos 7 dias</h2>
        @if (pontos().length) {
          <svg viewBox="0 0 700 220" role="img" aria-label="Gráfico de passos">
            <defs>
              <linearGradient id="linha" x1="0" x2="1" y1="0" y2="0">
                <stop offset="0%" stop-color="var(--vx-rose)" />
                <stop offset="55%" stop-color="var(--vx-primary)" />
                <stop offset="100%" stop-color="var(--vx-magenta)" />
              </linearGradient>
            </defs>
            <polyline [attr.points]="linhaSvg()" fill="none" stroke="url(#linha)" stroke-width="4" stroke-linejoin="round" stroke-linecap="round" />
            @for (p of pontos(); track p.data) {
              <circle [attr.cx]="p.x" [attr.cy]="p.y" r="6" fill="#fff" stroke="var(--vx-primary)" stroke-width="3">
                <title>{{ p.data | date: 'dd/MM' }}: {{ p.passos | number }} passos</title>
              </circle>
              <text [attr.x]="p.x" y="212" text-anchor="middle" class="eixo">{{ p.data | date: 'dd/MM' }}</text>
            }
          </svg>
        } @else {
          <p>Sem registros no período.</p>
        }
      </article>

      <article class="card">
        <h2>Últimos registros</h2>
        @for (r of registros(); track r.data) {
          <div class="linha-registro">
            <strong>{{ r.data | date: 'dd/MM' }}</strong>
            <span>Sono {{ r.horasSono | number: '1.1-1' }} h</span>
            <span>{{ r.passos | number }} passos</span>
            <span>{{ r.coposDeAgua }} copos</span>
            <span>Humor {{ r.humor }}/5</span>
          </div>
        }
      </article>
    </section>
  `,
  styles: `
    .cabecalho { display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap; }
    h1 { color: var(--vx-primary); margin: 0 0 1rem; font-weight: 500; }
    h2 { font-size: 1.1rem; margin: 0 0 0.75rem; color: var(--vx-foreground); }
    .btn { text-decoration: none; }
    .indicadores {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
      margin: 1.25rem 0;
    }
    .indicador { display: flex; flex-direction: column; gap: 0.25rem; padding: 1.1rem; }
    .indicador span { color: var(--vx-muted); font-size: 0.85rem; }
    .indicador strong { font-size: 1.4rem; color: var(--vx-magenta); }
    .grafico { margin-bottom: 1.25rem; }
    svg { width: 100%; height: auto; }
    .eixo { font-size: 14px; fill: var(--vx-muted); }
    .linha-registro {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
      padding: 0.6rem 0;
      border-bottom: 1px solid var(--vx-border);
      color: var(--vx-foreground);
    }
    .linha-registro:last-child { border-bottom: 0; }
    .linha-registro strong { min-width: 3.5rem; }
  `
})
export class PainelComponent implements OnInit {
  private readonly http = inject(HttpClient);

  readonly resumo = signal<ResumoPainel | null>(null);
  readonly registros = signal<RegistroDiario[]>([]);
  readonly erro = signal<string | null>(null);

  // Pontos do gráfico em ordem cronológica (mais antigo à esquerda)
  readonly pontos = computed(() => {
    const dados = [...this.registros()].reverse();
    if (!dados.length) return [];
    const max = Math.max(...dados.map((d) => d.passos), 1);
    return dados.map((d, i) => ({
      data: d.data,
      passos: d.passos,
      x: 40 + i * ((700 - 80) / Math.max(dados.length - 1, 1)),
      y: 180 - (d.passos / max) * 150
    }));
  });

  readonly linhaSvg = computed(() => this.pontos().map((p) => `${p.x},${p.y}`).join(' '));

  ngOnInit(): void {
    this.http.get<ResumoPainel>(`${environment.apiUrl}/painel/resumo`, { params: { dias: 7 } })
      .subscribe({
        next: (r) => this.resumo.set(r),
        error: () => this.erro.set('Não foi possível carregar o resumo.')
      });

    this.http.get<RegistroDiario[]>(`${environment.apiUrl}/registros`, { params: { dias: 7 } })
      .subscribe({
        next: (dados) => this.registros.set(dados),
        error: () => this.erro.set('Não foi possível carregar os registros.')
      });
  }
}
