namespace CLP.Entities
{
    public class PatternPoint
    {
        public PatternPoint()
        {
            StartHistoryActionIndex = -1;
            StartSemanticEventIndex = -1;
            EndHistoryActionIndex = -1;
            EndSemanticEventIndex = -1;
        }

        public string PageObjectID { get; set; }

        public int StartHistoryActionIndex { get; set; }
        public int StartSemanticEventIndex { get; set; }
        public int EndHistoryActionIndex { get; set; }
        public int EndSemanticEventIndex { get; set; }
        public string StartEventType { get; set; }
        public string EndEventType { get; set; }
    }
}