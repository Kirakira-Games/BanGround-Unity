namespace BGEditor
{
    public interface IEditorInfo
    {
        int barHeight { get; set; }
        int currentTimingGroup { get; set; }
        int gridDivision { get; set; }
        int maxHeight { get; }
        int numBeats { get; set; }
        float scrollPos { get; set; }
        bool isSpeedView { get; set; }
        EditorTool tool { get; set; }

        #region Fuwafuwa
        int yDivision { get; set; }
        bool yFilter { get; set; }
        float yPos { get; set; }
        #endregion

        #region SE
        bool isSEOn { get; set; }
        #endregion
    }
}
