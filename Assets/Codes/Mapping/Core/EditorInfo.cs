using System;

namespace BGEditor
{
    public enum EditorTool
    {
        Select, Single, Flick, Slide, Delete
    }

    public class EditorInfo : IEditorInfo
    {
        public const int MAX_BAR_HEIGHT = 600;
        public const int MIN_BAR_HEIGHT = 60;

        public int yDivision { get; set; }
        public float yPos { get; set; }
        public bool yFilter { get; set; }
        public bool speedView { get; set; }

        public int gridDivision { get; set; } = 4;
        public int barHeight { get; set; } = 300;
        public int numBeats { get; set; } = 100;

        public int currentTimingGroup { get; set; } = 0;
        public float scrollPos { get; set; } = 0;
        public EditorTool tool { get; set; } = EditorTool.Select;

        public int maxHeight => barHeight * numBeats;
    }
}
