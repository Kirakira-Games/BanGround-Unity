using UnityEngine;
using UnityEngine.Events;
using V2;

namespace BGEditor
{
    public interface IChartCore
    {
        Camera cam { get; }
        V2.Chart chart { get; }
        TimingGroup group { get; }
        UnityEvent onAudioLoaded { get; }
        UnityEvent onChartLoaded { get; }
        UnityEvent onGridModifed { get; }
        UnityEvent onGridMoved { get; }
        NoteEvent onNoteCreated { get; }
        NoteEvent onNoteModified { get; }
        NoteEvent onNoteRemoved { get; }
        NoteEvent onNoteYModified { get; }
        UnityEvent onSpeedViewSwitched { get; }
        UnityEvent onTimingGroupModified { get; }
        UnityEvent onTimingGroupSwitched { get; }
        UnityEvent onTimingModified { get; }
        UnityEvent onTimingPointModified { get; }
        UnityEvent onToolSwitched { get; }
        UnityEvent onUserChangeAudioProgress { get; }
        UnityEvent onYFilterSwitched { get; }
        UnityEvent onYPosModified { get; }
        ChangeEvent<int> onYSnapModified { get; }
        EditorToolTip tooltip { get; }
        MultiNoteDetector multinote { get; }

        bool Commit(IEditorCmd cmd);
        bool CreateNote(V2.Note note, bool allowMultiNote = false);
        void Exit();
        V2.Chart GetFinalizedChart();
        void IncreaseBarHeight(int delta);
        void LoadChart();
        void MoveGrid(float delta, bool force = false);
        void Redo();
        void RefreshBarCount();
        bool RemoveNote(V2.Note note);
        bool Rollback();
        bool RollbackRollback();
        void Save();
        void SeekGrid(float target, bool force = false);
        void SetGridDivision(int div);
        void SetY(float y);
        void SetYDivision(int div);
        void SwitchTool(EditorTool tool);
        void Undo();
        void AssignTimingGroups(V2.Chart chart);
        void 还没做好();
    }
}