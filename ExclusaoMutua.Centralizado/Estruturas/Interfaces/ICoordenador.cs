using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExclusaoMutua.Centralizado.Estruturas.Interfaces
{
    public interface ICoordenador
    {
        Task<bool> ConcederAcesso(IProcesso processo);
        void LiberarRecurso();
        void Morrer();
    }
}