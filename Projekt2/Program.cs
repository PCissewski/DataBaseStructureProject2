using Projekt2.btreeService;
using Projekt2.record;

namespace Projekt2
{
    class Program
    {
        static void Main(string[] args)
        {
            var bTreeService = new BTreeService(args[0]);
            bTreeService.PrintTree(args[0]);
            bTreeService.InsertRecord(new Record{ Key = 2, Person = "Jan Bach"}, args[0]);
            bTreeService.PrintTree(args[0]);
        }

    }
}