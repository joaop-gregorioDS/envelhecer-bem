export interface RegistroDiario {
  data: string;
  horasSono: number;
  passos: number;
  coposDeAgua: number;
  humor: number;
  observacao: string | null;
}

export interface ResumoPainel {
  dias: number;
  mediaSonoHoras: number;
  mediaPassos: number;
  mediaCoposDeAgua: number;
  mediaHumor: number;
  sequenciaAtual: number;
}

export type TipoLembrete = 'medicamento' | 'consulta';

export interface Lembrete {
  id: string;
  tipo: TipoLembrete;
  titulo: string;
  dataHoraUtc: string;
  observacao: string | null;
}
