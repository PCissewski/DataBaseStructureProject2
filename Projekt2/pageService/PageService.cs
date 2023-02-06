using System.Collections.Generic;
using System.IO;
using Projekt2.page;
using Projekt2.record;

namespace Projekt2.pageService
{
    public class PageService
    {

        /*
        Format strony:
        
        PointerRodzic##;
        PointerDziecko##;p_0
        Klucz#AdressToMainFile#PointerDziecko;
        Klucz#AdressToMainFile#PointerDziecko;
         */

        private readonly string _rootDirectory;
        private int _readCounter = 0;

        public PageService(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public Page LoadPage(int index)
        {
            _readCounter++;
            var pageData = GetPageData(index);
            
            var children = new List<int>();
            var records = new List<Record>();
            var parent = pageData[0][0] -'0';
            var n = 0;
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
                if (children.Contains(-1))
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i] = -1;
                    }
                }
                records.Add(new Record
                    {
                        LowerKeysPointer = children[n],
                        Person = data[1].TrimEnd('|'),
                        Key = int.Parse(data[0]),
                        GreaterKeysPointer = children[n + 1]
                    });
                n++;
            }

            var page = new Page
            {
                ChildrenIndexes = children,
                ParentIndex = parent,
                Records = records,
                PageIndex = index,
                PageData = pageData,
                isLeaf = children.Contains(-1)
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