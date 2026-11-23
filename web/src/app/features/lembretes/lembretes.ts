import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { environment } from '../../../environments/environment';
import { Lembrete } from '../../core/models/models';

@Component({
  selector: 'eb-lembretes',
  imports: [ReactiveFormsModule, DatePipe],
  template: `
    <section class="lembretes">
      <h1>Medicamentos e consultas</h1>

      @if (erro()) {
        <p class="alerta-erro" role="alert">{{ erro() }}</p>
      }

      <article class="card">
        <h2>Próximos lembretes</h2>
        @for (l of lembretes(); track l.id) {
          <div class="item">
            <span class="tipo" [class.consulta]="l.tipo === 'consulta'">{{ l.tipo === 'consulta' ? 'Consulta' : 'Remédio' }}</span>
            <div class="texto">
              <strong>{{ l.titulo }}</strong>
              <small>{{ l.dataHoraUtc | date: 'dd/MM/yyyy HH:mm' }}</small>
              @if (l.observacao) { <small class="obs">{{ l.observacao }}</small> }
            </div>
            <button class="remover" type="button" (click)="remover(l)" [attr.aria-label]="'Remover ' + l.titulo">Remover</button>
          </div>
        } @empty {
          <p>Nenhum lembrete cadastrado.</p>
        }
      </article>

      <article class="card novo">
        <h2>Novo lembrete</h2>
        <form [formGroup]="form" (ngSubmit)="adicionar()">
          <div class="grade">
            <div class="field">
              <label for="tipo">Tipo</label>
              <select id="tipo" formControlName="tipo">
                <option value="medicamento">Medicamento</option>
                <option value="consulta">Consulta</option>
              </select>
            </div>
            <div class="field">
              <label for="titulo">Título</label>
              <input id="titulo" formControlName="titulo" maxlength="120" />
              @if (form.controls.titulo.touched && form.controls.titulo.invalid) {
                <span class="erro">Informe um título.</span>
              }
            </div>
            <div class="field">
              <label for="quando">Data e hora</label>
              <input id="quando" type="datetime-local" formControlName="quando" />
            </div>
          </div>
          <div class="acoes">
            <button class="btn" type="submit" [disabled]="form.invalid">Adicionar</button>
          </div>
        </form>
      </article>
    </section>
  `,
  styles: `
    h1 { color: var(--vx-primary); margin: 0 0 1rem; font-weight: 500; }
    h2 { font-size: 1.1rem; margin: 0 0 0.75rem; }
    .novo { margin-top: 1.25rem; }
    .item {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid var(--vx-border);
    }
    .item:last-child { border-bottom: 0; }
    .texto { flex: 1; display: flex; flex-direction: column; }
    small { color: var(--vx-muted); }
    .obs { font-style: italic; }
    .tipo {
      background: var(--vx-gradient);
      color: #fff;
      padding: 0.25rem 0.6rem;
      border-radius: 999px;
      font-size: 0.8rem;
      font-weight: 600;
      white-space: nowrap;
    }
    .tipo.consulta { background: var(--vx-magenta); }
    .remover {
      background: none;
      border: 1px solid var(--vx-border);
      border-radius: var(--vx-radius);
      padding: 0.35rem 0.7rem;
      cursor: pointer;
      color: var(--vx-muted);
      font: inherit;
    }
    .remover:hover { color: var(--vx-error); border-color: var(--vx-error); }
    .grade { display: grid; gap: 0 1rem; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); }
    .acoes { display: flex; justify-content: flex-end; }
    .erro { color: var(--vx-error); font-size: 0.85rem; }
  `
})
export class LembretesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);

  readonly lembretes = signal<Lembrete[]>([]);
  readonly erro = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    tipo: ['medicamento' as 'medicamento' | 'consulta', [Validators.required]],
    titulo: ['', [Validators.required, Validators.maxLength(120)]],
    quando: [this.amanha(), [Validators.required]]
  });

  ngOnInit(): void {
    this.carregar();
  }

  adicionar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.http.post<Lembrete>(`${environment.apiUrl}/lembretes`, {
      tipo: v.tipo,
      titulo: v.titulo,
      // datetime-local vem sem fuso; tratamos como horário local e enviamos em UTC
      dataHoraUtc: new Date(v.quando).toISOString(),
      observacao: null
    }).subscribe({
      next: () => {
        this.form.patchValue({ titulo: '' });
        this.form.controls.titulo.markAsUntouched();
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível adicionar o lembrete.')
    });
  }

  remover(lembrete: Lembrete): void {
    this.http.delete(`${environment.apiUrl}/lembretes/${lembrete.id}`).subscribe({
      next: () => this.lembretes.update((lista) => lista.filter((l) => l.id !== lembrete.id)),
      error: () => this.erro.set('Não foi possível remover o lembrete.')
    });
  }

  private carregar(): void {
    this.http.get<Lembrete[]>(`${environment.apiUrl}/lembretes`).subscribe({
      next: (dados) => this.lembretes.set(dados),
      error: () => this.erro.set('Não foi possível carregar os lembretes.')
    });
  }

  private amanha(): string {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    d.setHours(9, 0, 0, 0);
    // formato aceito por input datetime-local (sem segundos)
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
}
