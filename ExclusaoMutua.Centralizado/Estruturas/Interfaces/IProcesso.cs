// Grupo:
//    Bruno Ricardo Junkes
//    Igor Christofer Eisenhut
//    Manoella Marcondes Junkes

using System.Threading.Tasks;

namespace ExclusaoMutua.Centralizado.Estruturas.Interfaces
{
    public interface IProcesso
    {
        int Pid { get; }
        void Processar();
    }
}