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
        private bool _isAlive = true;
        private bool _recursoSendoConsumido;
        private readonly ConcurrentQueue<IProcesso> _fila;

        public int Pid { get; }
        
        private static ICoordenador _coordenador;
        private readonly Random _random;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        

        public Processo(int pid)
        {
            Pid = pid;
            
            _fila = new();
            _random = new();
            _cancellationTokenSource = new();
            _cancellationToken = _cancellationTokenSource.Token;
        }


        // Coordenador -------------------------------------------------------------------------------------------------


        public async Task<bool> ConcederAcesso(IProcesso processo)
        {
            if (!_isAlive)
            {
                Console.WriteLine("** Coordenador indisponivel");
                return false;
            }

            if (_recursoSendoConsumido || !_fila.IsEmpty)
            {
                _fila.Enqueue(processo);
                Console.WriteLine($"[(Coordenador) {Pid}] Posto na fila: {processo.Pid}");

                while (_recursoSendoConsumido ||
                       _fila.TryPeek(out var proprocessoDaFila) && proprocessoDaFila != processo)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                _fila.TryDequeue(out _);
            }

            _recursoSendoConsumido = true;

            Console.WriteLine($"[(Coordenador) {Pid}] Acesso concedido: {processo.Pid}");
            return true;
        }

        public void LiberarRecurso()
        {
            if (!_isAlive)
            {
                Console.WriteLine("** Coordenador indisponivel");
            }
            
            Console.WriteLine($"[(Coordenador) {Pid}] Recurso liberado");
            _recursoSendoConsumido = false;
        }

        public void Morrer()
        {
            _isAlive = false;
        }


        // Processo ----------------------------------------------------------------------------------------------------


        public void Processar()
        {
            Console.WriteLine($"[{Pid}] Iniciando o processamento");
            Task.Factory.StartNew(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"[{Pid}] Solicitando acesso ao recurso...");
                    if (_coordenador != null && await _coordenador.ConcederAcesso(this))
                    {
                        Console.WriteLine($"[{Pid}] Consumindo o recurso...");
                        await Esperar(5, 15);
                        Console.WriteLine($"[{Pid}] Parou de consumir o recurso");

                        _coordenador.LiberarRecurso();
                    }
                    else
                    {
                        Console.WriteLine($"[{Pid}] Coordenador indisponível");
                    }

                    await Esperar(10, 25);
                }
            });
        }

        private async Task Esperar(int de, int ate)
        {
            var segundos = _random.Next(de, ate);
            await Task.Delay(TimeSpan.FromSeconds(segundos), _cancellationToken);
        }

        public void FinalizarProcessamento()
        {
            Console.WriteLine($"[{Pid}] Finalizando o processamento");
            _cancellationTokenSource.Cancel();
        }

        public void AtualizarCoordenador(ICoordenador coordenador)
        {
            if (coordenador == this)
            {
                FinalizarProcessamento();
            }
            
            _coordenador = coordenador;
        }
    }
}