using System;
using System.Collections.Generic;
using Projekt2.page;
using Projekt2.pageService;

namespace Projekt2.btreeService
{
    public class BTreeService
    {
        public void PrintTree(string[] args)
        {
            var pageService = new PageService();
            var indexes = new List<int>{0};

            while (indexes.Count > 0)
            {
                if (indexes[0] == -1)
                {
                    indexes.RemoveAt(0);
                    continue;
                }
                    
                var page = pageService.LoadPage(args, indexes[0]);
                indexes.RemoveAt(0);
                
                indexes.AddRange(page.ChildrenIndexes);

                Console.WriteLine($"Parent node: {page.ParentIndex}");
                PrintPage(page);
            }
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

        // TODO implement searching and inserting
    }
}