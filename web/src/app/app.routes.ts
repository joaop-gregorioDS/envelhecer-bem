import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/login/login';
import { PainelComponent } from './features/painel/painel';
import { RegistroComponent } from './features/registro/registro';
import { LembretesComponent } from './features/lembretes/lembretes';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'painel', component: PainelComponent, canActivate: [authGuard] },
  { path: 'registro', component: RegistroComponent, canActivate: [authGuard] },
  { path: 'lembretes', component: LembretesComponent, canActivate: [authGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'painel' },
  { path: '**', redirectTo: 'painel' }
];
