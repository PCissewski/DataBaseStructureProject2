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
                // insert record to a leaf
                PutRecord(page, record);
                FlushPage(page);
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
                    Compensation(page, leftSibling, parentPage, ancestorRecord, record, ancestorPointer, -1);
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
                    var ancestorRecord = parentPage.Records[parentPage.ChildrenIndexes.IndexOf(ancestorPointer)];
                    
                    // Perform compensation with right sibling
                    Compensation(rightSibling, page, parentPage, ancestorRecord, record, ancestorPointer, 0);
                    Console.WriteLine("Ok");
                    return;
                }
            }

            Console.WriteLine("Can't Compensate, perform Split");

            BasicSplit(page, parentPage, record);
            Console.WriteLine("Ok");
        }

        private void Compensation(Page overflownPage, Page sibling, Page parent, Record ancestorRecord, Record record, int ancestorPointer, int leftRight)
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
            parent.Records[parent.ChildrenIndexes.IndexOf(ancestorPointer) + leftRight] = middleRecord;
            
            parent.PageData[0] = parent.PageData[0] + ";\r\n";
            parent.PageData[1] = parent.PageData[1] + ";\r\n";
            
            ReplaceRecord(parent, ancestorRecord, middleRecord);
            TrimNewLine(parent);

            // Distribute records to children
            DistributeRecordsBetweenPages(sibling, overflownPage, records);
            
            TrimNewLine(sibling);
            TrimNewLine(overflownPage);
            
            FlushPage(parent);
            FlushPage(sibling);
            FlushPage(overflownPage);
        }

        private void BasicSplit(Page overflownPage, Page parentPage, Record record)
        {
            var records = new List<Record>{record};
            records.AddRange(overflownPage.Records);
            overflownPage.Records.Clear();
            records = records.OrderBy(r => r.Key).ToList();
            
            var fileCounter = 0;
            while (File.Exists(_rootDir + "\\page" + fileCounter + ".txt"))
            {
                fileCounter++;
            }

            var streamWriter = File.CreateText(_rootDir + "\\page" + fileCounter + ".txt");
            streamWriter.Write($"{parentPage.PageIndex}##;\r\n");
            streamWriter.Flush();
            streamWriter.Close();
            
            var newPage = new PageService(_rootDir).LoadPage(fileCounter);

            var middleRecord = records[records.Count / 2];
            
            middleRecord.GreaterKeysPointer = newPage.PageIndex;
            middleRecord.LowerKeysPointer = overflownPage.PageIndex;
            
            parentPage.ChildrenIndexes.Add(newPage.PageIndex);
            PutRecordParent(parentPage, middleRecord);

            DistributeRecordsBetweenPages(overflownPage, newPage, records);
            
            TrimNewLine(overflownPage);
            TrimNewLine(newPage);
            TrimNewLine(parentPage);
            
            FlushPage(parentPage);
            FlushPage(overflownPage);
            FlushPage(newPage);
        }

        private void DistributeRecordsBetweenPages(Page leftPage, Page rightPage, List<Record> records)
        {
            var leftPageData = new List<string>();
            var rightPageData = new List<string>();
            
            leftPageData.Add($"{leftPage.ParentIndex}##;\r\n");
            rightPageData.Add($"{rightPage.ParentIndex}##;\r\n");
            
            var middleRecord = records[records.Count / 2];
            var index = 0;

            var isFirstLeft = true;
            var isFirstRight = true;
            while (true)
            {
                var recordToPut = records[index];
                if (recordToPut.Key < middleRecord.Key)
                {
                    if (isFirstLeft)
                    {
                        leftPageData.Add($"{recordToPut.LowerKeysPointer}##;\r\n");
                        AddRecordToPageData(leftPageData, recordToPut);
                        leftPage.Records.Add(recordToPut);
                        isFirstLeft = false;
                        index++;
                        continue;
                    }
                    AddRecordToPageData(leftPageData, recordToPut);
                    leftPage.Records.Add(recordToPut);
                }
                else if (recordToPut.Key > middleRecord.Key)
                {
                    if (isFirstRight)
                    {
                        rightPageData.Add($"{recordToPut.LowerKeysPointer}##;\r\n");
                        AddRecordToPageData(rightPageData, recordToPut);
                        rightPage.Records.Add(recordToPut);
                        isFirstRight = false;
                        index++;
                        continue;
                    }
                    AddRecordToPageData(rightPageData, recordToPut);
                    rightPage.Records.Add(recordToPut);
                }

                index++;
                if (index == records.Count)
                    break;
            }

            rightPage.PageData = rightPageData.ToArray();
            leftPage.PageData = leftPageData.ToArray();
        }

        private void AddRecordToPageData(List<string> pageData, Record record)
        {
            pageData.Add($"{record.Key}#{record.Person}|#{record.GreaterKeysPointer};\r\n");
        }

        private void PutRecordParent(Page page, Record record)
        {
            var pageData = page.PageData.ToList();
            var records = new List<Record>();
            records.AddRange(page.Records);
            page.Records.Clear();
            records.Add(record);
            records = records.OrderBy(r => r.Key).ToList();
            var count = 0;
            
            pageData.Clear();
            pageData.Add($"{page.ParentIndex}##;\r\n");
            pageData.Add($"{records[0].LowerKeysPointer}##;\r\n");
            foreach (var r in records)
            {
                AddRecordToPageData(pageData, r);
                page.Records.Add(r);
            }

            page.PageData = pageData.ToArray();
        }
        
        private void PutRecord(Page page, Record record)
        {
            var children = new List<int>();
            var records = new List<Record>();
            
            foreach (var s in page.PageData)
            {
                var data = s.Split('#');
                
                if (data[0] == "")
                    break;

                if (int.Parse(data[0]) == 0 && data[1] == "" && data[2] == "")
                {
                    continue;
                }

                if (data[1] == "" && data[2] == "")
                {
                    children.Add(int.Parse(data[0]));
                    continue;
                }
                
                children.Add(int.Parse(data[2]));
                records.Add(new Record
                {
                    Person = data[1].TrimEnd('|'),
                    Key = int.Parse(data[0])
                });
            }
            records.Add(record);
            records = records.OrderBy(r=>r.Key).ToList();
            var header = page.PageData[0] + ";\r\n";
            var dummyPointer = records[0].GreaterKeysPointer + "##" + ";\r\n";
            var list = page.PageData.ToList();
            list.Clear();
            list.Add(header);
            list.Add(dummyPointer);
            foreach (var r in records)
            {
                list.Add(r.Key + "#" + r.Person + "|#" + r.LowerKeysPointer + ";\r\n");    
            }

            page.PageData = list.ToArray();
            
            TrimNewLine(page);
        }
        
        private void ReplaceRecord(Page parent, Record oldRecord, Record newRecord)
        {
            var toReplace = "";
            var replaceIndex = 0;
            var leave = false;

            var greaterNew = newRecord.GreaterKeysPointer;
            var lowerNew = newRecord.LowerKeysPointer;

            newRecord.GreaterKeysPointer = oldRecord.GreaterKeysPointer;
            newRecord.LowerKeysPointer = oldRecord.LowerKeysPointer;
            oldRecord.GreaterKeysPointer = greaterNew;
            oldRecord.LowerKeysPointer = lowerNew;

            foreach (var s in parent.PageData)
            {
                var split = s.Split("|");
                foreach (var s1 in split)
                {
                    if (s1 == oldRecord.Key + "#" + oldRecord.Person)
                    {
                        split[0] = newRecord.Key + "#" + newRecord.Person + "|";
                        toReplace = split[0] + split[1] + ";\r\n";
                        leave = true;
                        break;
                    }
                }

                if (leave)
                    break;
                replaceIndex++;
            }
            parent.PageData[replaceIndex] = toReplace;
            var n = 0;
            foreach (var s in parent.PageData)
            {
                if (!s.Contains(";\r\n"))
                {
                    parent.PageData[n] = parent.PageData[n] + ";\r\n";
                }

                n++;
            }

            if (parent.PageData[^1] == ";\r\n") parent.PageData[^1] = "";
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

        private void TrimNewLine(Page page)
        {
            if (page.PageData[^1] == "")
            {
                page.PageData[^2] = page.PageData[^2].TrimEnd('\n');
                page.PageData[^2] = page.PageData[^2].TrimEnd('\r');
                return;
            }
            page.PageData[^1] = page.PageData[^1].TrimEnd('\n');
            page.PageData[^1] = page.PageData[^1].TrimEnd('\r');
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