using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Projekt2.page;
using Projekt2.pageService;
using Projekt2.record;

namespace Projekt2.btreeService
{
    public class BTreeService
    {
        /*
        Format strony:
        
        PointerRodzic##;
        PointerDziecko##;p_0
        Klucz#ImieOsoby#PointerDziecko;
        Klucz#ImieOsoby#PointerDziecko;
         */
        public void PrintTree(string root)
        {
            var pageService = new PageService(root);
            var indexes = new List<int>{0};

            while (indexes.Count > 0)
            {
                if (indexes[0] == -1)
                {
                    indexes.RemoveAt(0);
                    continue;
                }
                    
                var page = pageService.LoadPage(indexes[0]);
                indexes.RemoveAt(0);
                
                indexes.AddRange(page.ChildrenIndexes);

                Console.WriteLine($"Parent node: {page.ParentIndex}");
                PrintPage(page);
            }
        }

        public Tuple<Record, bool, Page> SearchRecord(string root, int key)
        {
            var pageService = new PageService(root);
            var index = 0;
            var fileCount = Directory.EnumerateFiles(@"X:\InformatykaSemestr5\SBD\Project2\Projekt2\Projekt2\page", "*.txt", SearchOption.AllDirectories).Count();
            var isLeaf = false;
            var page = new Page();
            while (fileCount != 0 && index >= 0)
            {
                page = pageService.LoadPage(index);
                isLeaf = page.ChildrenIndexes.Contains(-1);
                var begin = 0;
                var last = page.Records.Count - 1;
                while (begin <= last)
                {
                    var middle = (begin + last) / 2;
                    if (key == page.Records[middle].Key)
                    {
                        Console.WriteLine("Found Record");
                        var record = page.Records[middle++];
                        Console.WriteLine(record.ToString());
                        return Tuple.Create(record, isLeaf, page);
                    }
                    if (key < page.Records[middle].Key)
                    {
                        last = middle - 1;
                        index = page.ChildrenIndexes[0];
                    }
                    else
                    {
                        begin = middle + 1;
                        index = page.ChildrenIndexes[begin];
                    }
                }

                fileCount--;
            }
            Console.WriteLine("Not Found");
            return Tuple.Create(new Record(), isLeaf, page);
        }
        
        private void PrintPage(Page page)
        {
            var number = 0;
            Console.WriteLine($"p_{number} = {page.ChildrenIndexes[number]}");
            foreach (var record in page.Records)
            {
                Console.WriteLine($"x_{number + 1} = {record.Key}");
                Console.WriteLine($"a_{number + 1} = {record.Person}");
                Console.WriteLine($"p_{number + 1} = {page.ChildrenIndexes[number + 1]}");
                number++;
            }
        }

        public void InsertRecord(Record record, string root)
        {
            var (searchedRecord, isLeaf, page) = SearchRecord(root, record.Key);
            
            if (searchedRecord.Key != 0)
            {
                Console.WriteLine("Already exist");
                return;
            }

            var currentPageIndex = 0;
            var pageService = new PageService(root);
            
            /*
            Format strony:
        
            PointerRodzic##;
            PointerDziecko##;p_0 dummyPointer
            Klucz#ImieOsoby#PointerDziecko; // 0 means its a root page
            Klucz#ImieOsoby#PointerDziecko; // -1 means no child
            */
            
            if (page.RecordsCount < Page.MaxRecords)
            {
                // insert record
                
                
                var recordString = "\r\n" + record.Key + "#" + record.Person + "#" + "-1" + ";";
                
                var streamWriter = File.AppendText(root + "\\page" + page.PageIndex + ".txt");

                streamWriter.Write(recordString);
                streamWriter.Flush();
                streamWriter.Close();
 
                Console.WriteLine("Ok");
                return;
            }

            Console.WriteLine("Compensation");
        }

        private string FetchChildPointer(string data)
        {
            return data.Split('#')[2];
        }
        
    }
}