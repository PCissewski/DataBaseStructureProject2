using System;
using System.IO;
using System.Linq;
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
            //InsertNRecords(10, bTreeService, args[0]);
            bTreeService.PrintTree();
        }

        private static void InsertNRecords(int recordsCount, BTreeService bts, string root)
        {
            var lastNamesPath = "X:/InformatykaSemestr5/SBD/Project2/Projekt2/Projekt2/lastNames.txt";
            var namesPath = "X:/InformatykaSemestr5/SBD/Project2/Projekt2/Projekt2/names.txt";
            
            var lnF = File.ReadAllText(lastNamesPath).Split('\n');
            var nF = File.ReadAllText(namesPath).Split('\n');

            var count = lnF.Length;
            
            var rand = new Random();
            
            for (int i = 0; i < recordsCount; i++)
            {
                bts.InsertRecord(new Record{Key = rand.Next(1, count), Person = $"{nF[rand.Next(1,count)]} {lnF[rand.Next(1,count)]}"}, root);
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
    }
}