namespace BGEditor
{
    public enum EditorTool
    {
        Select, Single, Flick, Slide, Delete
    }

    public class EditorInfo
    {
        public const int MAX_BAR_HEIGHT = 600;
        public const int MIN_BAR_HEIGHT = 60;


        public int yDivision;
        public float yPos;
        public bool yFilter;

        public int gridDivision;
        public int barHeight;
        public int numBeats;

        public int currentTimingGroup;
        public float scrollPos;
        public EditorTool tool;

        public int maxHeight => barHeight * numBeats;

        public EditorInfo()
        {
            gridDivision = 4;
            barHeight = 400;
            numBeats = 100;
            scrollPos = 0;
            currentTimingGroup = 0;
            tool = EditorTool.Select;
        }
    }
}
