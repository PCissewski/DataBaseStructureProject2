using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        private readonly string _rootDir;

        public BTreeService(string rootDir)
        {
            _rootDir = rootDir;
        }

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

        public Tuple<Record, bool, Page, int> SearchRecord(string root, int key)
        {
            var pageService = new PageService(root);
            var index = 0;
            var fileCount = Directory.EnumerateFiles(@"X:\InformatykaSemestr5\SBD\Project2\Projekt2\Projekt2\page", "*.txt", SearchOption.AllDirectories).Count();
            var isLeaf = false;
            var page = new Page();
            var ancestorPointer = -1;
            while (fileCount != 0 && index >= 0)
            {
                page = pageService.LoadPage(index);
                ancestorPointer = index;
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
                        return Tuple.Create(record, isLeaf, page, ancestorPointer);
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
            return Tuple.Create(new Record(), isLeaf, page, ancestorPointer);
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
            var (searchedRecord, isLeaf, page, ancestorPointer) = SearchRecord(root, record.Key);
            
            if (searchedRecord.Key != 0)
            {
                Console.WriteLine("Already exist");
                return;
            }
            
            if (page.RecordsCount < Page.MaxRecords)
            {
                // insert record
                var recordString = "\r\n" + record.Key + "#" + record.Person + "|#" + "-1" + ";";
                
                var streamWriter = File.AppendText(root + "\\page" + page.PageIndex + ".txt");

                streamWriter.Write(recordString);
                streamWriter.Flush();
                streamWriter.Close();
 
                Console.WriteLine("Ok");
                return;
            }
            
            Console.WriteLine("Try Compensation");
            // First, check if compensation is possible
            var ps = new PageService(root);
            var parentPage = ps.LoadPage(page.ParentIndex);
            // Check whether left exists
            var isLeftSibling =  parentPage.ChildrenIndexes.Contains(ancestorPointer - 1); // for right ancestorPointer + 1
            // If exists check whether it is full, if it is compensation with this sibling is impossible
            if (isLeftSibling)
            {
                var leftSibling = ps.LoadPage(ancestorPointer - 1);
                
                if (leftSibling.RecordsCount < Page.MaxRecords)
                {
                    var ancestorRecord = parentPage.Records[parentPage.ChildrenIndexes.IndexOf(ancestorPointer) - 1];
                    
                    // Perform compensation with left sibling
                    Compensation(page, leftSibling, parentPage, ancestorRecord, record, ancestorPointer);
                    Console.WriteLine("Ok");
                    return;
                }
            }
            // Same for right sibling
            
            // Check whether right exists
            var isRightSibling =  parentPage.ChildrenIndexes.Contains(ancestorPointer + 1);
            // If exists check whether it is full, if it is compensation with this sibling is impossible
            if (isRightSibling)
            {
                var rightSibling = ps.LoadPage(ancestorPointer + 1);
                
                if (rightSibling.RecordsCount < Page.MaxRecords)
                {
                    var ancestorRecord = parentPage.Records[parentPage.ChildrenIndexes.IndexOf(ancestorPointer) - 1];
                    
                    // Perform compensation with right sibling
                    Compensation(page, rightSibling, parentPage, ancestorRecord, record, ancestorPointer);
                    Console.WriteLine("Ok");
                    return;
                }
            }

            Console.WriteLine("Can't Compensate, perform Split");
            
            var records = new List<Record>();
        }

        private void Compensation(Page overflownPage, Page sibling, Page parent, Record ancestorRecord, Record record, int ancestorPointer)
        {
            Console.WriteLine("Running Compensation");
            // Put all records to a single list
            var records = new List<Record>();
            foreach (var overflownPageRecord in overflownPage.Records)
            {
                records.Add(overflownPageRecord);
            }
            overflownPage.Records.Clear();

            foreach (var siblingRecord in sibling.Records)
            {
                records.Add(siblingRecord);
            }
            sibling.Records.Clear();
            
            records.Add(ancestorRecord);
            records.Add(record);
            
            records = records.OrderBy(r => r.Key).ToList();
            
            // Find middle record in sequence and swap with ancestorRecord in parent page
            var middleRecord = records[records.Count / 2];
            parent.Records[parent.ChildrenIndexes.IndexOf(ancestorPointer) - 1] = middleRecord;
            
            parent.PageData[0] = parent.PageData[0] + ";\r\n";
            parent.PageData[1] = parent.PageData[1] + ";\r\n";
            
            ReplaceRecord(parent, ancestorRecord, middleRecord);
            
            // Distribute records to children
            var counter = 0;
            var siblingStrings = new List<string> 
                {sibling.PageData[0]+ ";\r\n", sibling.PageData[1]+ ";\r\n"};
            while (counter < records.Count / 2)
            {
                // put records left from middle record into page
                sibling.Records.Add(records[counter]);
                if (sibling.isLeaf)
                {
                    sibling.PageData = PutRecordLeaf(siblingStrings, records[counter]);
                }
                else
                {
                    PutRecordParent(); // TODO napisac to
                }
                
                counter++;
            }

            var ovStrings = new List<string> 
                {overflownPage.PageData[0] + ";\r\n", overflownPage.PageData[1] + ";\r\n"};
            counter++;
            while (counter < records.Count)
            {
                // put records right from middle record into page
                overflownPage.Records.Add(records[counter]);
                if (overflownPage.isLeaf)
                {
                    overflownPage.PageData = PutRecordLeaf(ovStrings, records[counter]);
                }
                else
                {
                    PutRecordParent(); // TODO napisac to
                }
                counter++;
            }

            FlushPage(parent);
            FlushPage(sibling);
            FlushPage(overflownPage);
        }

        private string[] PutRecordLeaf(List<string> pageData, Record record)
        {
            var stringRecord = record.Key + "#" + record.Person + "|#-1;\r\n";
            pageData.Add(stringRecord);
            return pageData.ToArray();
        }

        private void PutRecordParent()
        {
            
        }
        
        private void ReplaceRecord(Page parent, Record oldRecord, Record newRecord)
        {
            var toReplace = "";
            var replaceIndex = 0;
            var leave = false;

            foreach (var s in parent.PageData)
            {
                var split = s.Split("|");
                foreach (var s1 in split)
                {
                    if (s1 == oldRecord.Key + "#" + oldRecord.Person)
                    {
                        split[0] = newRecord.Key + "#" + newRecord.Person + "|";
                        toReplace = split[0] + split[1] + ";";
                        leave = true;
                        break;
                    }
                }

                if (leave)
                    break;
                replaceIndex++;
            }

            parent.PageData[replaceIndex] = toReplace;
        }

        private void FlushPage(Page page)
        {
            var streamWriter = File.CreateText(_rootDir + "\\page" + page.PageIndex + ".txt");
            var final = page.PageData.Aggregate("", (current, s) => current + s);
            
            streamWriter.Write(final);
            streamWriter.Flush();
            streamWriter.Close();
        }
    }
}