namespace Projekt2.record
{
    public class Record
    {
        public int LowerKeysPointer { get; set; } = -1;
        public int GreaterKeysPointer { get; set; } = -1;
        public string Person { get; set; }
        public int Key { get; set; }

        public override string ToString()
        {
            return "Person: " + Person + " Key: " + Key;
        }
    }
}