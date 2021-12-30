namespace Projekt2.record
{
    public class Record
    {
        public string Person { get; set; }
        public int Key { get; set; }

        public override string ToString()
        {
            return "Person: " + Person + " Key: " + Key;
        }
    }
}