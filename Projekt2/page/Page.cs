using System.Collections.Generic;
using Projekt2.record;

namespace Projekt2.page
{
    public class Page
    {
        public int ParentIndex { get; set; }
        public List<int> ChildrenIndexes { get; set; }
        public List<Record> Records { get; set; }
        public int RecordsCount => Records.Count;
    }
    
}