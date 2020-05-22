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

        public int gridDivision;
        public int barHeight;
        public int numBars;
        public float scrollPos;
        public EditorTool tool;

        public int maxHeight => barHeight * numBars;

        public EditorInfo()
        {
            gridDivision = 4;
            barHeight = 60;
            numBars = 100;
            scrollPos = 0;
            tool = EditorTool.Select;
        }
    }
}
