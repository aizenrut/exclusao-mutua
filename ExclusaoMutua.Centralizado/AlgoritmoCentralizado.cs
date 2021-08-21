using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExclusaoMutua.Centralizado.Estruturas;
using ExclusaoMutua.Centralizado.Estruturas.Interfaces;

namespace ExclusaoMutua.Centralizado
{
    public class AlgoritmoCentralizado : IDisposable
    {
        private readonly Random _random;
        private readonly List<Processo> _processos;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        private Processo _coordenador;

        public AlgoritmoCentralizado()
        {
            _processos = new();
            _cancellationTokenSource = new();
            _cancellationToken = _cancellationTokenSource.Token;
            _random = new();
        }

        public async Task Run()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    CriarNovoProcesso().Processar();

                    await Task.Delay(TimeSpan.FromSeconds(40));
                }
            });

            await Task.Delay(1000); // Para dar tempo de criar o primeiro processo

            Task.Factory.StartNew(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    DefinirNovoCoordenador();

                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
        }

        private Processo CriarNovoProcesso()
        {
            Console.WriteLine("** Criando novo processo");

            var pid = ObterNovoPid();
            var processo = new Processo(pid);

            _processos.Add(processo);

            Console.WriteLine($"** Quantidade de processos ativos: {_processos.Count} ({string.Join(", ", _processos.Select(x => x.Pid))})");
            
            return processo;
        }

        private int ObterNovoPid()
        {
            int novoPid;

            do
            {
                novoPid = _random.Next();
            } while (_processos.Any(x => x.Pid == novoPid));

            return novoPid;
        }

        private void DefinirNovoCoordenador()
        {
            if (_coordenador != null)
                MatarCoordenador();

            ElegerNovoCoordenador();

            NotificarNovoCoordenador();
        }

        private void MatarCoordenador()
        {
            Console.WriteLine("** Matando o coordenador");

            _coordenador.Morrer();
            _processos.Remove(_coordenador);
        }

        private void ElegerNovoCoordenador()
        {
            var indiceAleatorio = _random.Next(0, _processos.Count - 1);
            var novoCoordenador = _processos.ElementAt(indiceAleatorio);

            Console.WriteLine($"** Novo coordenador eleito: {novoCoordenador.Pid}");

            _coordenador = novoCoordenador;
        }

        private void NotificarNovoCoordenador()
        {
            Console.WriteLine($"** Notificando processos sobre o novo coordenador");

            foreach (var processo in _processos)
            {
                processo.AtualizarCoordenador(_coordenador);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}