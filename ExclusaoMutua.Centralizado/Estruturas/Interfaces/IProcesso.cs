using System.Threading.Tasks;

namespace ExclusaoMutua.Centralizado.Estruturas.Interfaces
{
    public interface IProcesso
    {
        int Pid { get; }
        void Processar();
    }
}