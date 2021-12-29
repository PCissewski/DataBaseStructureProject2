using System.Collections.Generic;
using System.IO;
using Projekt2.page;
using Projekt2.record;

namespace Projekt2.pageService
{
    public class PageService
    {
        public Page LoadPage(string[] args, int index)
        {
            var pageData = File.ReadAllText(args[index]).Replace("\r\n", "").Split(';');
            
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
                Records = records
            };
            var i = 1;
            foreach (var child in children)
            {
                if (child == -1)
                {
                    i++;
                    continue;
                }
                    
                var pg = LoadPage(args, i);
                i++;
            }
            
            return page;
        }
    }
}