using Projekt2.btreeService;
using Projekt2.record;

namespace Projekt2
{
    class Program
    {
        static void Main(string[] args)
        {
            var bTreeService = new BTreeService();
            bTreeService.PrintTree(args[0]);
            bTreeService.InsertRecord(new Record{ Key = 7, Person = "Wolfgang Amadeus Mozart"}, args[0]);
        }

    }
}