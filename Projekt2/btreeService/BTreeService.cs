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
        private int _readCounter = 0;
        private int _writeCounter = 0;
        public BTreeService(string rootDir)
        {
            _rootDir = rootDir;
        }

        public void PrintTree()
        {
            var pageService = new PageService(_rootDir);
            var indexes = new List<int>{0};

            while (indexes.Count > 0)
            {
                if (indexes[0] == -1)
                {
                    indexes.RemoveAt(0);
                    continue;
                }
                    
                var page = pageService.LoadPage(indexes[0]);
                _readCounter++;
                indexes.RemoveAt(0);
                
                indexes.AddRange(page.ChildrenIndexes);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Parent node: {page.ParentIndex}");
                Console.WriteLine($"Page Index: {page.PageIndex}");
                if (page.ChildrenIndexes.Count == 0) return;
                Console.ForegroundColor = ConsoleColor.Green;
                PrintPage(page);
                Console.ResetColor();
            }
        }

        public Tuple<Record, bool, Page, Record> SearchRecord(string root, int key)
        {
            var pageService = new PageService(root);
            var index = 0;
            var fileCount = Directory.EnumerateFiles(@"X:\Studia\InformatykaSemestr5\SBD\Project2\Projekt2\Projekt2\page", "*.txt", SearchOption.AllDirectories).Count();
            var isLeaf = false;
            var page = new Page();
            var ancestorRecord = new Record();
            while (fileCount != 0 && index >= 0)
            {
                page = pageService.LoadPage(index);
                _readCounter++;
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
                        return Tuple.Create(record, isLeaf, page, ancestorRecord);
                    }
                    if (key < page.Records[middle].Key)
                    {
                        last = middle - 1;
                        index = page.Records[middle].LowerKeysPointer;
                        if (page.isLeaf) continue;
                        ancestorRecord = page.Records[middle];
                        var i = page.Records.IndexOf(ancestorRecord);
                        ancestorRecord.LowerKeysPointer = page.ChildrenIndexes[i];
                        ancestorRecord.GreaterKeysPointer = page.ChildrenIndexes[i + 1];
                    }
                    else
                    {
                        begin = middle + 1;
                        index = page.Records[middle].GreaterKeysPointer;
                        if (page.isLeaf) continue;
                        ancestorRecord = page.Records[middle];
                        var i = page.Records.IndexOf(ancestorRecord);
                        ancestorRecord.LowerKeysPointer = page.ChildrenIndexes[i];
                        ancestorRecord.GreaterKeysPointer = page.ChildrenIndexes[i + 1];
                    }
                }

                fileCount--;
            }
            Console.WriteLine("Not Found");
            return Tuple.Create(new Record{Key = -1}, isLeaf, page, ancestorRecord);
        }

        public void InsertRecord(Record record)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Inserting: {record}");
            Console.ResetColor();
            var (searchedRecord, isLeaf, pageWhereSearchedForRecord, toSwapRecord) = SearchRecord(_rootDir, record.Key);
            if (searchedRecord.Key != -1)
            {
                Console.WriteLine("Already exist");
                return;
            }
            
            if (pageWhereSearchedForRecord.RecordsCount < Page.MaxRecords)
            {
                // insert record to a leaf
                PutRecord(pageWhereSearchedForRecord, record);
                FlushPage(pageWhereSearchedForRecord);
                Console.WriteLine("Ok");
                return;
            }
            
            Console.WriteLine("Try Compensation");
            // First, check if compensation is possible
            var ps = new PageService(_rootDir);
            var parentPage = ps.LoadPage(pageWhereSearchedForRecord.ParentIndex);
            _readCounter++;
            var isRoot = pageWhereSearchedForRecord.PageIndex == parentPage.ParentIndex;
            // Check whether left exists
            //var isLeftSibling =  parentPage.ChildrenIndexes.Contains(ancestorPointer - 1); // for right ancestorPointer + 1
            
            // find page index whose siblings we will check if they exist later
            var pageWhereSearchedForRecordIndex =
                parentPage.ChildrenIndexes.IndexOf(pageWhereSearchedForRecord.PageIndex);
            var isLeftSibling = false;
            var leftSiblingIndex = -1;
            try
            { 
                leftSiblingIndex = parentPage.ChildrenIndexes[pageWhereSearchedForRecordIndex - 1];
                isLeftSibling = true;
            }
            catch (Exception _)
            {
                Console.WriteLine("Left sibling does not exist");
            }

            if (leftSiblingIndex == -1)
            {
                isLeftSibling = false;
            }
            
            //var isLeftSibling = toSwapRecord.LowerKeysPointer != -1 && pageWhereSearchedForRecord.PageIndex != toSwapRecord.LowerKeysPointer;
            // If exists check whether it is full, if it is compensation with this sibling is impossible
            if (isLeftSibling && !isRoot)
            {
                var leftSibling = ps.LoadPage(leftSiblingIndex);
                _readCounter++;
                if (leftSibling.RecordsCount < Page.MaxRecords)
                {
                    
                    if (toSwapRecord.LowerKeysPointer == leftSiblingIndex){}
                    else
                    {
                        // var i = parentPage.Records.IndexOf(toSwapRecord);
                        int i = parentPage.Records.TakeWhile(rec => rec.Key != toSwapRecord.Key).Count();
                        toSwapRecord = parentPage.Records[i - 1];
                    }
                    
                    // Perform compensation with left sibling
                    Compensation(pageWhereSearchedForRecord, leftSibling, parentPage, toSwapRecord, record);
                    Console.WriteLine("Ok");
                    return;
                }
            }
            // Same for right sibling
            
            // Check whether right exists
            var isRightSibling = false;
            var rightSiblingIndex = -1;
            try
            { 
                rightSiblingIndex = parentPage.ChildrenIndexes[pageWhereSearchedForRecordIndex + 1];
                isRightSibling = true;
            }
            catch (Exception _)
            {
                Console.WriteLine("Right sibling does not exist");
            }
            //var isRightSibling = toSwapRecord.GreaterKeysPointer != -1 && pageWhereSearchedForRecord.PageIndex != toSwapRecord.GreaterKeysPointer;
            // If exists check whether it is full, if it is compensation with this sibling is impossible

            if (rightSiblingIndex == -1)
            {
                isRightSibling = false;
            }
            
            if (isRightSibling && !isRoot)
            {
                var rightSibling = ps.LoadPage(rightSiblingIndex);
                _readCounter++;
                if (rightSibling.RecordsCount < Page.MaxRecords)
                {
                    
                    if (toSwapRecord.GreaterKeysPointer == rightSiblingIndex){}
                    else
                    {
                        // var i = parentPage.Records.IndexOf(toSwapRecord);
                        int i = parentPage.Records.TakeWhile(rec => rec.Key != toSwapRecord.Key).Count();
                        toSwapRecord = parentPage.Records[i + 1];
                    }
                    
                    // Perform compensation with right sibling
                    Compensation(rightSibling, pageWhereSearchedForRecord, parentPage, toSwapRecord, record);
                    Console.WriteLine("Ok");
                    return;
                }
            }

            Console.WriteLine("Can't Compensate, perform Split");

            if (pageWhereSearchedForRecord.ParentIndex == pageWhereSearchedForRecord.PageIndex)
            {
                Console.WriteLine("Split Root");
                RootSplit(pageWhereSearchedForRecord, record);
                Console.WriteLine("Root has been split");
                return;
            }

            BasicSplit(pageWhereSearchedForRecord, parentPage, record);
            
            Console.WriteLine("Ok");
        }

        private void RootSplit(Page page, Record record)
        {
            var records = new List<Record> {record};
            records.AddRange(page.Records);
            records = records.OrderBy(r => r.Key).ToList();
            
            // create 2 pages
            
            var newPageLeft = CreateNewChildPage(page);
            
            var newPageRight = CreateNewChildPage(page);
            
            // put middle to parent so in this case to "page"
            
            var middleRecord = records[records.Count / 2];
            
            middleRecord.GreaterKeysPointer = newPageRight.PageIndex;
            middleRecord.LowerKeysPointer = newPageLeft.PageIndex;

            page.ChildrenIndexes.Add(newPageLeft.PageIndex);
            page.ChildrenIndexes.Add(newPageRight.PageIndex);
            
            page.Records.Clear();
            PutRecordParent(page, middleRecord);
            
            DistributeRecordsBetweenPages(newPageLeft, newPageRight, records);
            
            TrimNewLine(newPageLeft);
            TrimNewLine(newPageRight);
            
            FlushPage(newPageLeft);
            FlushPage(newPageRight);
            FlushPage(page);
        }

        private int FileNumber()
        {
            var fileCounter = 0;
            while (File.Exists(_rootDir + "\\page" + fileCounter + ".txt"))
            {
                fileCounter++;
            }

            return fileCounter;
        }

        private Page CreateNewChildPage(Page parent)
        {
            var fileCounter = FileNumber();
            
            var streamWriter = File.CreateText(_rootDir + "\\page" + fileCounter + ".txt");
            streamWriter.Write($"{parent.PageIndex}##;\r\n");
            streamWriter.Flush();
            streamWriter.Close();
            
            return new PageService(_rootDir).LoadPage(fileCounter);
        }
        
        private void Compensation(Page overflownPage, Page sibling, Page parent, Record ancestorRecord, Record record)
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
            //parent.Records[ancestorPointer] = middleRecord;
            
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
            
            records = records.OrderBy(r => r.Key).ToList();
            
            if (parentPage.RecordsCount + 1 > Page.MaxRecords)
            {
                Console.WriteLine("Split parent");
                SplitParent(parentPage, overflownPage, record);
                
                return;
            }
            
            overflownPage.Records.Clear();
            
            var fileCounter = FileNumber();

            var streamWriter = File.CreateText(_rootDir + "\\page" + fileCounter + ".txt");
            streamWriter.Write($"{parentPage.PageIndex}##;\r\n");
            streamWriter.Flush();
            streamWriter.Close();
            
            var newPage = new PageService(_rootDir).LoadPage(fileCounter);
            _readCounter++;
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

        private void SplitParent(Page parentPage, Page overflownPage, Record record)
        {
            var (childPageOfMainParent, middleRecord, newPageL, newPageR ) = Split(overflownPage, record);
            
            // page to jest rodzic tych dwoch nowych stron newPageL i newPageR
            // page to tez dziecko parentPage
            
            var records = new List<Record> {middleRecord};
            records.AddRange(parentPage.Records);
            records = records.OrderBy(r => r.Key).ToList(); // tu wszystkie rekordy main parenta
            
            var olderParent =  new PageService(_rootDir).LoadPage(parentPage.ParentIndex);
            
            if (olderParent.PageIndex != parentPage.PageIndex)
            {
                var rightChild = parentPage;
                parentPage = olderParent;
                rightChild.ParentIndex = parentPage.ParentIndex;
                rightChild.PageData[0] = $"{parentPage.PageIndex}##;\r\n";
                
                childPageOfMainParent.ParentIndex = parentPage.ParentIndex;
                childPageOfMainParent.PageData[0] = $"{parentPage.PageIndex}##;\r\n";

                if (parentPage.RecordsCount + 1 > Page.MaxRecords)
                {
                    Console.WriteLine("Split parent");
                    SplitParent(parentPage, overflownPage, record);
                
                    return;
                }
                
                var midRecordIndex = records.IndexOf(middleRecord);
                if (midRecordIndex != 0 && midRecordIndex != records.Count -1)
                {
                    records[midRecordIndex - 1].GreaterKeysPointer = middleRecord.LowerKeysPointer;
                    records[midRecordIndex + 1].LowerKeysPointer = middleRecord.GreaterKeysPointer;
                }

                var midRecordParent = records[records.Count / 2];
                
                var middleIndex = records.IndexOf(midRecordParent);
                records[middleIndex - 1].GreaterKeysPointer = midRecordParent.LowerKeysPointer;
                records[middleIndex + 1].LowerKeysPointer = midRecordParent.GreaterKeysPointer;
                rightChild.Records.Clear();
                DistributeRecordsBetweenPages(childPageOfMainParent, rightChild, records);
                
                midRecordParent.GreaterKeysPointer = rightChild.PageIndex;
                midRecordParent.LowerKeysPointer = childPageOfMainParent.PageIndex;
                
                //parentPage.Records.Clear();
                parentPage.ChildrenIndexes.Clear();
                var temp = new List<Record>();
                temp.AddRange(parentPage.Records);
                temp.Add(midRecordParent);
                temp = temp.OrderBy(r => r.Key).ToList();
                var toPutRecordIndex = temp.IndexOf(midRecordParent);

                if (toPutRecordIndex != 0 && toPutRecordIndex != temp.Count - 1)
                {
                    parentPage.Records[toPutRecordIndex - 1].GreaterKeysPointer = midRecordParent.LowerKeysPointer;
                    parentPage.Records[toPutRecordIndex + 1].LowerKeysPointer = midRecordParent.GreaterKeysPointer;
                }

                if (toPutRecordIndex == 0)
                {
                    parentPage.Records[toPutRecordIndex + 1].LowerKeysPointer = midRecordParent.GreaterKeysPointer;
                }

                if (toPutRecordIndex == temp.Count - 1)
                {
                    parentPage.Records[toPutRecordIndex - 1].GreaterKeysPointer = midRecordParent.LowerKeysPointer;
                }
                
                
                PutRecordParent(parentPage, midRecordParent);
                
                VerifyKids(childPageOfMainParent);
                VerifyKids(rightChild);
                
                TrimNewLine(parentPage);
                TrimNewLine(rightChild);
                TrimNewLine(childPageOfMainParent);
            
                FlushPage(parentPage);
                FlushPage(rightChild);
                FlushPage(childPageOfMainParent);
                
                return;
            }
            
            var middleRecordIndex = records.IndexOf(middleRecord);
            if (middleRecordIndex != 0 && middleRecordIndex != records.Count -1)
            {
                records[middleRecordIndex - 1].GreaterKeysPointer = middleRecord.LowerKeysPointer;
                records[middleRecordIndex + 1].LowerKeysPointer = middleRecord.GreaterKeysPointer;
            }
            // create 1 page

            var newPageRight = CreateNewChildPage(parentPage);
            
            var pageDataChild = childPageOfMainParent.PageData.ToList();
            pageDataChild.Clear();
            pageDataChild.Add($"{parentPage.PageIndex}##;\r\n");

            var middleRecordNewParent = records[records.Count / 2];
            
            var midIndex = records.IndexOf(middleRecordNewParent);
            records[midIndex - 1].GreaterKeysPointer = middleRecordNewParent.LowerKeysPointer;
            records[midIndex + 1].LowerKeysPointer = middleRecordNewParent.GreaterKeysPointer;
            
            DistributeRecordsBetweenPages(childPageOfMainParent, newPageRight, records);
            
            middleRecordNewParent.GreaterKeysPointer = newPageRight.PageIndex;
            middleRecordNewParent.LowerKeysPointer = childPageOfMainParent.PageIndex;
            
            parentPage.Records.Clear();
            parentPage.ChildrenIndexes.Clear();
            // tu ewentualnie ma sie zmienic parent page
            
            // tutaj kiedys bylo ta rekurencja
            
            PutRecordParent(parentPage, middleRecordNewParent);
            
            // na tym etapie main parent ma w sobie rekord ze wskaznikami na swoje dzieci
            
            // teraz trzeba rozdystrybuwoac i ustawic odpowiednio wskazniki
            
            // var ps = new PageService(_rootDir);
            //
            // foreach (var childPageRecord in childPageOfMainParent.Records)
            // {
            //     var pageGreater = ps.LoadPage(childPageRecord.GreaterKeysPointer);
            //     var pageLower = ps.LoadPage(childPageRecord.LowerKeysPointer);
            //     pageGreater.PageData[0] = $"{childPageOfMainParent.PageIndex}##;\r\n";
            //     for (int i = 1; i < pageGreater.PageData.Length; i++)
            //     {
            //         pageGreater.PageData[i] += ";\r\n";
            //     }
            //     
            //     pageLower.PageData[0] = $"{childPageOfMainParent.PageIndex}##;\r\n";
            //     for (int i = 1; i < pageLower.PageData.Length; i++)
            //     {
            //         pageLower.PageData[i] += ";\r\n";
            //     }
            //     TrimNewLine(pageLower);
            //     TrimNewLine(pageGreater);
            //     FlushPage(pageLower);
            //     FlushPage(pageGreater);
            //     _writeCounter-=2;
            // }
            
            VerifyKids(childPageOfMainParent);
            VerifyKids(newPageRight);
            // foreach (var r in newPageRight.Records)
            // {
            //     var pageL = ps.LoadPage(r.LowerKeysPointer);
            //     var pageR = ps.LoadPage(r.GreaterKeysPointer);
            //     //_readCounter++;
            //     //_readCounter++;
            //     pageL.PageData[0] = $"{newPageRight.PageIndex}##;\r\n";
            //     for (int i = 1; i < pageL.PageData.Length; i++)
            //     {
            //         pageL.PageData[i] += ";\r\n";
            //     }
            //     
            //     pageR.PageData[0] = $"{newPageRight.PageIndex}##;\r\n";
            //     for (int i = 1; i < pageR.PageData.Length; i++)
            //     {
            //         pageR.PageData[i] += ";\r\n";
            //     }
            //
            //     pageL.PageData[^1] = "";
            //     pageR.PageData[^1] = "";
            //     TrimNewLine(pageL);
            //     TrimNewLine(pageR);
            //     FlushPage(pageL);
            //     FlushPage(pageR);
            //     _writeCounter -= 2;
            // }
            
            TrimNewLine(parentPage);
            TrimNewLine(newPageRight);
            TrimNewLine(childPageOfMainParent);
            
            FlushPage(parentPage);
            FlushPage(newPageRight);
            FlushPage(childPageOfMainParent);
            
        }

        private Tuple<Page, Record, Page, Page> Split(Page parentPage, Record record)
        {
            var records = new List<Record>{record};
            records.AddRange(parentPage.Records);
            parentPage.Records.Clear();
            records = records.OrderBy(r => r.Key).ToList();

            var newPageLeft = CreateNewChildPage(parentPage);
            
            var newPageRight = CreateNewChildPage(parentPage);

            var middleRecord = records[records.Count / 2];
            
            middleRecord.GreaterKeysPointer = newPageRight.PageIndex;
            middleRecord.LowerKeysPointer = newPageLeft.PageIndex;
            
            parentPage.ChildrenIndexes.Add(newPageRight.PageIndex);
            parentPage.ChildrenIndexes.Add(newPageLeft.PageIndex);
            
            //PutRecordParent(parentPage, middleRecord);
            
            newPageLeft.ParentIndex = parentPage.PageIndex;
            newPageRight.ParentIndex = parentPage.PageIndex;
            
            DistributeRecordsBetweenPages(newPageLeft, newPageRight, records);
            
            TrimNewLine(newPageLeft);
            TrimNewLine(newPageRight);
            
            FlushPage(newPageLeft);
            FlushPage(newPageRight);
            
            return Tuple.Create(parentPage, middleRecord, newPageLeft, newPageRight);
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
                if (index == records.Count) break;
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

        private void VerifyKids(Page parent)
        {
            var ps = new PageService(_rootDir);

            foreach (var childPageRecord in parent.Records)
            {
                var pageGreater = ps.LoadPage(childPageRecord.GreaterKeysPointer);
                var pageLower = ps.LoadPage(childPageRecord.LowerKeysPointer);
                pageGreater.PageData[0] = $"{parent.PageIndex}##;\r\n";
                for (int i = 1; i < pageGreater.PageData.Length; i++)
                {
                    pageGreater.PageData[i] += ";\r\n";
                }
                
                pageLower.PageData[0] = $"{parent.PageIndex}##;\r\n";
                for (int i = 1; i < pageLower.PageData.Length; i++)
                {
                    pageLower.PageData[i] += ";\r\n";
                }
                TrimNewLine(pageLower);
                TrimNewLine(pageGreater);
                FlushPage(pageLower);
                FlushPage(pageGreater);
                _writeCounter-=2;
            }
        }
        
        private void PutRecordParent(Page page, Record record)
        {
            var pageData = page.PageData.ToList();
            var records = new List<Record>{record};
            records.AddRange(page.Records);
            page.Records.Clear();
            records = records.OrderBy(r => r.Key).ToList();

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

            if (page.PageData[^1] == ";") page.PageData[^1] = "";
            if (page.PageData.Length > 1)
                if (page.PageData[^2] == ";") page.PageData[^2] = "";
            
            var final = page.PageData.Aggregate("", (current, s) => current + s);

            streamWriter.Write(final);
            streamWriter.Flush();
            streamWriter.Close();
            _writeCounter++;
        }
        
        public int GetReadCounter()
        {
            return _readCounter;
        }

        public int GetWriteCounter()
        {
            return _writeCounter;
        }
    }
}