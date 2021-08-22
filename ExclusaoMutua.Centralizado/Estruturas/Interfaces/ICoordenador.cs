// Grupo:
//    Bruno Ricardo Junkes
//    Igor Christofer Eisenhut
//    Manoella Marcondes Junkes

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExclusaoMutua.Centralizado.Estruturas.Interfaces
{
    public interface ICoordenador
    {
        Task<bool> ConcederAcesso(Processo processo);
        bool LiberarRecurso(Processo processo);
        void Morrer();
    }
}