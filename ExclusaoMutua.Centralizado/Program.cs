// Grupo:
//    Bruno Ricardo Junkes
//    Igor Christofer Eisenhut
//    Manoella Marcondes Junkes

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExclusaoMutua.Centralizado
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Pressione [ENTER] para finalizar\n");

            using (var algoritmo = new AlgoritmoCentralizado())
            {
                algoritmo.Run();
                Console.ReadLine();
            }
        }
    }
}