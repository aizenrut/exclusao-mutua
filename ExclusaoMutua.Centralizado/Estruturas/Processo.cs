using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExclusaoMutua.Centralizado.Estruturas.Interfaces;

namespace ExclusaoMutua.Centralizado.Estruturas
{
    public class Processo : IProcesso, ICoordenador
    {
        private bool _ativo = true;
        private readonly ConcurrentQueue<IProcesso> _fila;
        private readonly Random _random;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public static Processo Coordenador;
        public static bool RecursoLiberado = true;
        
        public int Pid { get; }

        public Processo(int pid)
        {
            Pid = pid;

            _fila = new();
            _random = new();
            _cancellationTokenSource = new();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        #region Processo

        public void Processar()
        {
            LogProcesso("Iniciou o processamento");
            Task.Factory.StartNew(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    var pidCoordenador = Coordenador.Pid;

                    LogProcesso("Solicitando acesso ao recurso...");
                    if (Coordenador != null && await Coordenador.ConcederAcesso(this))
                    {
                        await ConsumirRecurso();

                        LogProcesso("Notificando liberação...");
                        if (pidCoordenador != Coordenador.Pid || !Coordenador.LiberarRecurso(this))
                        {
                            LogProcesso("Coordenador indisponível");
                        }
                    }
                    else
                    {
                        LogProcesso($"Coordenador indisponível");
                    }

                    await Esperar(10, 25);
                }
            });
        }

        private async Task ConsumirRecurso()
        {
            try
            {
                RecursoLiberado = false;
                LogProcesso("Consumindo o recurso...");

                await Esperar(5, 15);
            }
            finally
            {
                LogProcesso("Parou de consumir o recurso");
                RecursoLiberado = true;
            }
        }

        private async Task Esperar(int de, int ate)
        {
            var segundos = _random.Next(de, ate);
            await Task.Delay(TimeSpan.FromSeconds(segundos), _cancellationToken);
        }

        private void LogProcesso(string mensagem)
        {
            Console.WriteLine($"[{Pid}] {mensagem}");
        }
        
        #endregion
        
        #region Coordenador

        public async Task<bool> ConcederAcesso(Processo processo)
        {
            if (!_ativo)
            {
                return false;
            }

            if (!RecursoLiberado || !_fila.IsEmpty)
            {
                _fila.Enqueue(processo);
                LogCoordenador($"Posto na fila: {string.Join(", ", _fila.ToArray().Select(x => x.Pid))}");

                while (!RecursoLiberado || _fila.TryPeek(out var processoDaFila) && processoDaFila != processo)
                {
                    if (!_ativo)
                        return false;

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                _fila.TryDequeue(out _);
            }

            LogCoordenador($"Acesso concedido: {processo.Pid}");
            return true;
        }

        public bool LiberarRecurso(Processo processo)
        {
            if (!_ativo)
            {
                return false;
            }

            LogCoordenador($"Recurso liberado: {processo.Pid}");

            return true;
        }

        public void Morrer()
        {
            _ativo = false;
            _cancellationTokenSource.Cancel();
        }

        private void LogCoordenador(string mensagem)
        {
            Console.WriteLine($"\t\t\t\t\t\t\t\t\t\t\t\t\t[{Pid} (Coordenador)] {mensagem}");
        }
        
        #endregion
    }
}