namespace BGEditor
{
    public enum EditorTool
    {
        Select, Single, Flick, Slide, Delete
    }

    public class EditorInfo
    {
        public const int MAX_BAR_HEIGHT = 500;
        public const int MIN_BAR_HEIGHT = 50;

        public int gridDivision;
        public int barHeight;
        public int numBars;
        public float scrollPos;
        public EditorTool tool;

        public int maxHeight => barHeight * numBars;

        public EditorInfo()
        {
            gridDivision = 4;
            barHeight = 500;
            numBars = 100;
            scrollPos = 0;
            tool = EditorTool.Select;
        }
    }
}
