using Projekt2.btree;
using Projekt2.btreeService;
using Projekt2.pageService;

namespace Projekt2
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootPage = new PageService();
            var bTreeService = new BTreeService();
            var bTree = new BTree
            {
                Root = rootPage.LoadPage(args, 0)
            };
            
            bTreeService.PrintTree(args);
        }

    }
}