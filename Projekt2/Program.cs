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
            bTreeService.InsertRecord(new Record{ Key = 10, Person = "Karol Draszawka"}, args[0]);
            bTreeService.InsertRecord(new Record{ Key = 15, Person = "Jakub Hirsz"}, args[0]);
            bTreeService.InsertRecord(new Record{ Key = 27, Person = "Debil"}, args[0]);
            bTreeService.InsertRecord(new Record{ Key = 18, Person = "Kacper Bartkowski"}, args[0]);
            bTreeService.InsertRecord(new Record{ Key = 13, Person = "Jan Daciuk"}, args[0]);
            bTreeService.InsertRecord(new Record{ Key = 30, Person = "Hans Kloss"}, args[0]);
            bTreeService.PrintTree(args[0]);
        }

    }
}