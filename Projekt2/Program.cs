using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Projekt2.btreeService;
using Projekt2.record;

namespace Projekt2
{
    class Program
    {
        static void Main(string[] args)
        {
            // args[0] => path to a directory with Btree pages 
            var bTreeService = new BTreeService(args[0]);
            ResetTree(args[0]);
            bTreeService.PrintTree();
            var N = 100;
            //InsertNRecords(N, bTreeService);
            InsertManuallyTest(bTreeService);
            bTreeService.PrintTree();
        }

        private static void InsertNRecords(int recordsCount, BTreeService bts)
        {
            var lastNamesPath = "X:/Studia/InformatykaSemestr5/SBD/Project2/Projekt2/Projekt2/lastNames.txt";
            var namesPath = "X:/Studia/InformatykaSemestr5/SBD/Project2/Projekt2/Projekt2/names.txt";
            var logFile = "X:/Studia/InformatykaSemestr5/SBD/Project2/Projekt2/Projekt2/log.txt";
            
            var lnF = File.ReadAllText(lastNamesPath).Split('\n');
            var nF = File.ReadAllText(namesPath).Split('\n');
            var count = lnF.Length;
            
            var rand = new Random();

            for (int i = 0; i < recordsCount; i++)
            {
                Console.WriteLine($"Iteration : {i}");
                var record = new Record
                    {Key = rand.Next(1, count), Person = $"{nF[rand.Next(1, count)]} {lnF[rand.Next(1, count)]}"};
                var toTest = $"bts.InsertRecord(new Record{{ Person = \"{record.Person}\", Key = {record.Key} }});";
                var fs = File.AppendText(logFile);
                fs.Write($"{toTest}\n");
                fs.Flush();
                fs.Close();
                bts.InsertRecord(record);
                //bts.PrintTree();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("-----------------------------------------------------");
                Console.ResetColor();
            }
            
        }

        private static void ResetTree(string root)
        {
            var fileCount = Directory.EnumerateFiles(root, "*.txt", SearchOption.AllDirectories).Count();
            for (int i = 0; i < fileCount; i++)
            {
                File.Delete(root + "\\page" + i + ".txt");
            }
            var streamWriter = File.CreateText(root + "\\page" + 0 + ".txt");
            streamWriter.Write("0##;");
            streamWriter.Flush();
            streamWriter.Close();
        }

        private static void InsertManuallyTest(BTreeService bts)
        {
            bts.InsertRecord(new Record{ Person = "Joel MAYS", Key = 345 });
            bts.InsertRecord(new Record{ Person = "Ashlie HEWITT", Key = 765 });
            bts.InsertRecord(new Record{ Person = "Ronald BLANCHARD", Key = 438 });
            bts.InsertRecord(new Record{ Person = "Jordan MATHIS", Key = 6 });
            bts.InsertRecord(new Record{ Person = "Juliana MCDONALD", Key = 373 });
            bts.InsertRecord(new Record{ Person = "Kerry PITTS", Key = 743 });
            bts.InsertRecord(new Record{ Person = "Brianne HOPPER", Key = 800 });
            bts.InsertRecord(new Record{ Person = "Alfredo SHANNON", Key = 496 });
            bts.InsertRecord(new Record{ Person = "Shawn BAUER", Key = 244 });
            bts.InsertRecord(new Record{ Person = "Justine VINCENT", Key = 877 });
            bts.InsertRecord(new Record{ Person = "Evelyn MORTON", Key = 528 });
            bts.InsertRecord(new Record{ Person = "Teresa FULLER", Key = 817 });
            bts.InsertRecord(new Record{ Person = "Latasha BENSON", Key = 708 });
            bts.InsertRecord(new Record{ Person = "Mallory ROGERS", Key = 33 });
            bts.InsertRecord(new Record{ Person = "Stacey BARRON", Key = 802 });
            bts.InsertRecord(new Record{ Person = "Erick MARSH", Key = 797 });
            bts.InsertRecord(new Record{ Person = "Cassandra TILLMAN", Key = 86 });
            bts.InsertRecord(new Record{ Person = "Tabatha MAYS", Key = 471 });
            bts.InsertRecord(new Record{ Person = "Bryant STAFFORD", Key = 581 });
            bts.InsertRecord(new Record{ Person = "Alicia CONTRERAS", Key = 137 });
            bts.InsertRecord(new Record{ Person = "Helen HUDSON", Key = 734 });
            bts.InsertRecord(new Record{ Person = "Brett BLACKBURN", Key = 27 });
            bts.InsertRecord(new Record{ Person = "Christen VEGA", Key = 806 });
            bts.InsertRecord(new Record{ Person = "Jose YATES", Key = 999 });
            bts.InsertRecord(new Record{ Person = "Kellen LEACH", Key = 457 });
            bts.InsertRecord(new Record{ Person = "Freddie HUGHES", Key = 457 });
            bts.InsertRecord(new Record{ Person = "Jodi MORENO", Key = 492 });
            bts.InsertRecord(new Record{ Person = "Omar CARROLL", Key = 232 });
            bts.InsertRecord(new Record{ Person = "Marshall BUCKNER", Key = 773 });
            bts.InsertRecord(new Record{ Person = "Heather WOODARD", Key = 749 });
            bts.InsertRecord(new Record{ Person = "Bret GREGORY", Key = 819 });
            bts.InsertRecord(new Record{ Person = "Gerald HOLMAN", Key = 877 });
            bts.InsertRecord(new Record{ Person = "Roy BANKS", Key = 245 });
            bts.InsertRecord(new Record{ Person = "Deborah IRWIN", Key = 979 });
            bts.InsertRecord(new Record{ Person = "Kendrick FRANKLIN", Key = 203 });
            bts.InsertRecord(new Record{ Person = "Amie DELEON", Key = 964 });
            bts.InsertRecord(new Record{ Person = "Roland BRYAN", Key = 964 });
            bts.InsertRecord(new Record{ Person = "Bobbi HOWE", Key = 542 });
            bts.InsertRecord(new Record{ Person = "Teresa PRESTON", Key = 771 });
            bts.InsertRecord(new Record{ Person = "Jackie BENJAMIN", Key = 69 });
            bts.InsertRecord(new Record{ Person = "Julia CHANG", Key = 485 });
            bts.InsertRecord(new Record{ Person = "Blair SAVAGE", Key = 136 });
            bts.InsertRecord(new Record{ Person = "Melanie WEAVER", Key = 452 });
            bts.InsertRecord(new Record{ Person = "Janice ROSARIO", Key = 967 });
            bts.InsertRecord(new Record{ Person = "Owen OCONNOR", Key = 285 });
            bts.InsertRecord(new Record{ Person = "Chris OLIVER", Key = 610 });
            bts.InsertRecord(new Record{ Person = "Malcolm BRIDGES", Key = 49 });
            bts.InsertRecord(new Record{ Person = "Charlie FULLER", Key = 923 });
            bts.InsertRecord(new Record{ Person = "Marina GOLDEN", Key = 596 });
            bts.InsertRecord(new Record{ Person = "Alaina BRADLEY", Key = 568 });
            bts.InsertRecord(new Record{ Person = "Camille DORSEY", Key = 910 });
            bts.InsertRecord(new Record{ Person = "Dante WHITNEY", Key = 827 });
            bts.InsertRecord(new Record{ Person = "Marquita GLOVER", Key = 980 });
            bts.InsertRecord(new Record{ Person = "Latoya JENKINS", Key = 147 });
            bts.InsertRecord(new Record{ Person = "Theresa ARNOLD", Key = 37 });
            bts.InsertRecord(new Record{ Person = "Janine PETTY", Key = 531 });
            bts.InsertRecord(new Record{ Person = "Dominick PITTS", Key = 633 });
            bts.InsertRecord(new Record{ Person = "Alecia MOSS", Key = 472 });
            bts.InsertRecord(new Record{ Person = "Ronnie CHRISTENSEN", Key = 410 });
            bts.InsertRecord(new Record{ Person = "Roman WALTON", Key = 840 });
            bts.InsertRecord(new Record{ Person = "Natalia KIRK", Key = 870 });
            bts.InsertRecord(new Record{ Person = "Marisa CHERRY", Key = 144 });
            bts.InsertRecord(new Record{ Person = "Elyse GOODMAN", Key = 756 });
            bts.InsertRecord(new Record{ Person = "Ricky WATERS", Key = 926 });
            bts.InsertRecord(new Record{ Person = "Dean MCCARTHY", Key = 360 });
            bts.InsertRecord(new Record{ Person = "Alicia SULLIVAN", Key = 263 });
            bts.InsertRecord(new Record{ Person = "Hilary HEWITT", Key = 729 });
            bts.InsertRecord(new Record{ Person = "Allen MIRANDA", Key = 177 });
            bts.InsertRecord(new Record{ Person = "Antonio CARNEY", Key = 728 });
            bts.InsertRecord(new Record{ Person = "Seth VINCENT", Key = 415 });
            bts.InsertRecord(new Record{ Person = "Nicole GARRETT", Key = 27 });
            bts.InsertRecord(new Record{ Person = "Ashly BOONE", Key = 44 });
            bts.InsertRecord(new Record{ Person = "Joyce GRIMES", Key = 680 });
            bts.InsertRecord(new Record{ Person = "Jillian MOSLEY", Key = 647 });
            bts.InsertRecord(new Record{ Person = "Grace HARRINGTON", Key = 662 });
            bts.InsertRecord(new Record{ Person = "Stuart BRITT", Key = 88 });
            bts.InsertRecord(new Record{ Person = "Alberto VAZQUEZ", Key = 961 });
            bts.InsertRecord(new Record{ Person = "Lucy DUDLEY", Key = 228 });
            bts.InsertRecord(new Record{ Person = "Juliana PARKER", Key = 557 });
            bts.InsertRecord(new Record{ Person = "Cory BURKE", Key = 209 });
            bts.InsertRecord(new Record{ Person = "Margaret STEWART", Key = 924 });
            bts.InsertRecord(new Record{ Person = "Allison KERR", Key = 784 });
            bts.InsertRecord(new Record{ Person = "Bradford VELEZ", Key = 138 });
            bts.InsertRecord(new Record{ Person = "Krystina PAUL", Key = 74 });
            bts.InsertRecord(new Record{ Person = "Kristin RIVERS", Key = 445 });
            bts.InsertRecord(new Record{ Person = "Nikki REID", Key = 717 });
            bts.InsertRecord(new Record{ Person = "Donna OLSON", Key = 592 });

        }

        private static void GetCounters(BTreeService bTreeService, int n)
        {
            Console.WriteLine($"Reads: {bTreeService.GetReadCounter()}");
            Console.WriteLine($"Write: {bTreeService.GetWriteCounter()}");
            Console.WriteLine($"Average: {(bTreeService.GetReadCounter() + bTreeService.GetWriteCounter())/n}");
        }
    }
}