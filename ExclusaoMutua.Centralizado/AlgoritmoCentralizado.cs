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
            
                    await Task.Delay(TimeSpan.FromSeconds(40), _cancellationToken);
                }
            });
            
            await Task.Delay(1000); // Para dar tempo de criar o primeiro processo
            
            Task.Factory.StartNew(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    DefinirNovoCoordenador();
            
                    await Task.Delay(TimeSpan.FromMinutes(1), _cancellationToken);
                }
            });
        }

        private Processo CriarNovoProcesso()
        {
            var pid = ObterNovoPid();
            var processo = new Processo(pid);
            
            Log($"Criando novo processo: {processo.Pid}");

            _processos.Add(processo);

            Log($"Processos ativos: {string.Join(", ", _processos.Select(x => x.Pid))}");
            
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
            if (Processo.Coordenador != null)
                MatarCoordenador();

            var processo = ElegerNovoCoordenador();

            NotificarNovoCoordenador(processo);
        }

        private void MatarCoordenador()
        {
            Log($"Matando o coordenador: {Processo.Coordenador.Pid}");

            Processo.Coordenador.Morrer();
            _processos.Remove(Processo.Coordenador);
        }

        private Processo ElegerNovoCoordenador()
        {
            var indiceAleatorio = _random.Next(0, _processos.Count - 1);
            var novoCoordenador = _processos.ElementAt(indiceAleatorio);

            Log($"Coordenador eleito: {novoCoordenador.Pid}");

            return novoCoordenador;
        }

        private void NotificarNovoCoordenador(Processo processo)
        {
            Log("Notificando processos sobre o novo coordenador");

            Processo.Coordenador = processo;
        }

        private void Log(string mensagem)
        {
            Console.WriteLine($"\t\t\t\t\t\t** {mensagem}");
        }
        
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}