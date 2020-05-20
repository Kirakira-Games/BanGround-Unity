namespace BGEditor
{
    public class EditorInfo
    {
        public const int MAX_BAR_HEIGHT = 400;
        public const int MIN_BAR_HEIGHT = 30;

        public int gridDivision;
        public int barHeight;
        public int numBars;
        public int maxHeight => barHeight * numBars;

        public EditorInfo()
        {
            gridDivision = 4;
            barHeight = 100;
            numBars = 100;
        }
    }
}
