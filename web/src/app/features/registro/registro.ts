import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { RegistroDiario } from '../../core/models/models';

@Component({
  selector: 'eb-registro',
  imports: [ReactiveFormsModule],
  template: `
    <section class="card formulario">
      <h1>Registro diário</h1>
      <p class="sub">Preencha com o que aconteceu hoje. Um registro por dia; salvar de novo substitui o do mesmo dia.</p>

      @if (erro()) {
        <p class="alerta-erro" role="alert">{{ erro() }}</p>
      }

      <form [formGroup]="form" (ngSubmit)="salvar()">
        <div class="grade">
          <div class="field">
            <label for="data">Data</label>
            <input id="data" type="date" formControlName="data" />
          </div>
          <div class="field">
            <label for="sono">Horas de sono</label>
            <input id="sono" type="number" step="0.5" min="0" max="24" formControlName="horasSono" />
          </div>
          <div class="field">
            <label for="passos">Passos</label>
            <input id="passos" type="number" min="0" max="100000" formControlName="passos" />
          </div>
          <div class="field">
            <label for="agua">Copos de água</label>
            <input id="agua" type="number" min="0" max="30" formControlName="coposDeAgua" />
          </div>
        </div>

        <fieldset class="humor">
          <legend>Como você se sentiu?</legend>
          @for (n of [1, 2, 3, 4, 5]; track n) {
            <label class="opcao">
              <input type="radio" [value]="n" formControlName="humor" />
              <span>{{ n }}</span>
            </label>
          }
        </fieldset>

        <div class="field">
          <label for="obs">Observação (opcional)</label>
          <textarea id="obs" rows="3" maxlength="500" formControlName="observacao"></textarea>
        </div>

        <div class="acoes">
          <button class="btn" type="submit" [disabled]="salvando()">
            {{ salvando() ? 'Salvando...' : 'Salvar registro' }}
          </button>
        </div>
      </form>
    </section>
  `,
  styles: `
    .formulario { max-width: 720px; margin: 0 auto; }
    h1 { color: var(--vx-primary); margin: 0 0 0.25rem; font-weight: 500; }
    .sub { color: var(--vx-muted); margin: 0 0 1.5rem; }
    .grade { display: grid; gap: 0 1rem; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); }
    .humor { border: 0; padding: 0; margin: 0 0 1rem; }
    .humor legend { font-weight: 600; margin-bottom: 0.5rem; }
    .opcao { display: inline-flex; align-items: center; gap: 0.3rem; margin-right: 1rem; cursor: pointer; }
    .acoes { display: flex; justify-content: flex-end; }
  `
})
export class RegistroComponent {
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    data: [new Date().toISOString().slice(0, 10), [Validators.required]],
    horasSono: [7, [Validators.required, Validators.min(0), Validators.max(24)]],
    passos: [5000, [Validators.required, Validators.min(0), Validators.max(100000)]],
    coposDeAgua: [6, [Validators.required, Validators.min(0), Validators.max(30)]],
    humor: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
    observacao: ['']
  });

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.salvando.set(true);
    this.erro.set(null);

    this.http.post<RegistroDiario>(`${environment.apiUrl}/registros`, {
      data: v.data,
      horasSono: v.horasSono,
      passos: v.passos,
      coposDeAgua: v.coposDeAgua,
      humor: v.humor,
      observacao: v.observacao || null
    }).subscribe({
      next: () => this.router.navigateByUrl('/painel'),
      error: (err: HttpErrorResponse) => {
        this.salvando.set(false);
        this.erro.set(err.status === 400
          ? 'Confira os valores: alguma informação está fora do limite.'
          : 'Não foi possível salvar o registro.');
      }
    });
  }
}
