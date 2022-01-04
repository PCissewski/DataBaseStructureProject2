using System.Collections.Generic;
using System.IO;
using Projekt2.page;
using Projekt2.record;

namespace Projekt2.pageService
{
    public class PageService
    {
        private readonly string _rootDirectory;

        public PageService(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public Page LoadPage(int index)
        {
            var pageData = GetPageData(index);
            
            var children = new List<int>();
            var records = new List<Record>();
            var parent = 0;

            foreach (var s in pageData)
            {
                var data = s.Split('#');
                
                if (data[0] == "")
                    break;

                if (int.Parse(data[0]) == 0 && data[1] == "" && data[2] == "")
                {
                    parent = 0;
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
                        Person = data[1],
                        Key = int.Parse(data[0])
                    });
            }

            var page = new Page
            {
                ChildrenIndexes = children,
                ParentIndex = parent,
                Records = records,
                PageIndex = index
            };

            return page;
        }

        private string[] GetPageData(int index)
        {
            return File.ReadAllText(_rootDirectory + "\\page" + index + ".txt")
                       .Replace("\r\n", "")
                       .Split(';');
        }
    }
}