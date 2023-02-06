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
            bts.InsertRecord(new Record{ Person = "Nolan GALLAGHER", Key = 81 });
            bts.InsertRecord(new Record{ Person = "Ismael CHAVEZ", Key = 592 });
            bts.InsertRecord(new Record{ Person = "Carolina RUSH", Key = 858 });
            bts.InsertRecord(new Record{ Person = "Colleen CAIN", Key = 633 }); 
            bts.InsertRecord(new Record{ Person = "Stefanie TYLER", Key = 698 });
            bts.InsertRecord(new Record{ Person = "Bridget BOLTON", Key = 266 });
            bts.InsertRecord(new Record{ Person = "Mandy MONTOYA", Key = 883 });
            bts.InsertRecord(new Record{ Person = "Teri HOPKINS", Key = 274 });
            bts.InsertRecord(new Record{ Person = "Saul MORAN", Key = 920 });
            bts.InsertRecord(new Record{ Person = "Kellie WILLIS", Key = 495 });
            bts.InsertRecord(new Record{ Person = "Alexandria SANTANA", Key = 306 });
            bts.InsertRecord(new Record{ Person = "Wendy BENSON", Key = 284 });
            bts.InsertRecord(new Record{ Person = "Olivia FIGUEROA", Key = 535 });
            bts.InsertRecord(new Record{ Person = "Latonya WILLIAMS", Key = 98 });
            bts.InsertRecord(new Record{ Person = "Shaun MATHIS", Key = 357 });
            bts.InsertRecord(new Record{ Person = "Sean WORKMAN", Key = 553 });
            bts.InsertRecord(new Record{ Person = "Leon SAWYER", Key = 404 });
            bts.InsertRecord(new Record{ Person = "Chance WILEY", Key = 185 });
            bts.InsertRecord(new Record{ Person = "Adriana BLACK", Key = 881 });
            bts.InsertRecord(new Record{ Person = "Ashlie TYSON", Key = 254 });
            bts.InsertRecord(new Record{ Person = "Marc DAUGHERTY", Key = 108 });
            bts.InsertRecord(new Record{ Person = "Courtney KENNEDY", Key = 926 });
            // bts.InsertRecord(new Record{ Person = "Daniel VAUGHAN", Key = 138 });
            // bts.InsertRecord(new Record{ Person = "Elijah MERRILL", Key = 601 });
            // bts.InsertRecord(new Record{ Person = "Latisha BRADY", Key = 647 });
            // bts.InsertRecord(new Record{ Person = "Nichole VINSON", Key = 903 });
        //    bts.InsertRecord(new Record{ Person = "Robyn CHASE", Key = 829 });


        }

        private static void GetCounters(BTreeService bTreeService, int n)
        {
            Console.WriteLine($"Reads: {bTreeService.GetReadCounter()}");
            Console.WriteLine($"Write: {bTreeService.GetWriteCounter()}");
            Console.WriteLine($"Average: {(bTreeService.GetReadCounter() + bTreeService.GetWriteCounter())/n}");
        }
    }
}