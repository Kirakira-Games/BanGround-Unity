using System;
using UnityEngine;
using Zenject;

namespace BGEditor
{
    public enum EditorTool
    {
        Select, Single, Flick, Slide, Delete
    }

    public class EditorInfoFactory : IFactory<IEditorInfo>
    {
        private EditorInfo mSavedInstance;

        private IEditorInfo TestEditorInfo()
        {
            return new EditorInfo(this)
            {
                yDivision = 3,
                yPos = 3,
                yFilter = true,
                isSpeedView = true,
                gridDivision = 16,
                barHeight = 100,
                numBeats = 100,
                currentTimingGroup = 1,
                scrollPos = 1000,
                tool = EditorTool.Delete,
                isSEOn = false
            };
        }

        public IEditorInfo Create()
        {
            //return TestEditorInfo();
            var ret = mSavedInstance;
            if (ret == null)
                return new EditorInfo(this);

            mSavedInstance = null;
            return ret;
        }

        public void Save(EditorInfo info)
        {
            Debug.Assert(mSavedInstance == null, "[EditorInfo] Overwriting saved instance!");
            mSavedInstance = info;
        }
    }

    public class EditorInfo : IEditorInfo
    {
        public readonly EditorInfoFactory factory;

        public EditorInfo(EditorInfoFactory factory)
        {
            this.factory = factory;
        }

        public const int MAX_BAR_HEIGHT = 600;
        public const int MIN_BAR_HEIGHT = 60;

        public int yDivision { get; set; }
        public float yPos { get; set; }
        public bool yFilter { get; set; }
        public bool isSpeedView { get; set; }

        public int gridDivision { get; set; } = 4;
        public int barHeight { get; set; } = 300;
        public int numBeats { get; set; } = 100;

        public int currentTimingGroup { get; set; } = 0;
        public float scrollPos { get; set; } = 0;
        public EditorTool tool { get; set; } = EditorTool.Select;

        public int maxHeight => barHeight * numBeats;

        public int beatSnapIndex { get; set; } = 3;
        public bool isSEOn { get; set; } = true;

        public void Save()
        {
            factory.Save(this);
        }
    }
}
