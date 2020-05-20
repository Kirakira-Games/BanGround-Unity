namespace BGEditor
{
    public class EditorInfo
    {
        public const int MAX_BAR_HEIGHT = 400;
        public const int MIN_BAR_HEIGHT = 50;

        public int gridDivision;
        public int barHeight;
        public int numBars;
        public float scrollPos;

        public int maxHeight => barHeight * numBars;

        public EditorInfo()
        {
            gridDivision = 6;
            barHeight = 200;
            numBars = 100;
            scrollPos = 1000;
        }
    }
}
