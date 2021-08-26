using BanGround;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using BanGround.Scene.Params;
using BanGround.Game.Mods;
using NoteType = V2.NoteType;

namespace BGEditor
{
    public class BoolEvent : UnityEvent<bool> { }
    public class NoteEvent : UnityEvent<V2.Note> { }
    public class ChangeEvent<T>: UnityEvent<T, T> { }

    public class ChartCore : MonoBehaviour, IChartCore
    {
        [Inject]
        private IAudioManager audioManager;
        [Inject]
        private IDataLoader dataLoader;
        [Inject]
        private IChartLoader chartLoader;
        [Inject]
        private IEditorInfo editor;
        [Inject]
        private IObjectPool pool;
        [Inject(Id = "Blocker")]
        private Button Blocker;
        [Inject]
        private IAudioProgressController progress;
        [Inject]
        private IEditNoteController notes;
        [Inject]
        private IMessageBannerController messageBannerController;
        [Inject]
        private IMessageBox messageBox;
        [Inject]
        private IFileSystem fs;

        public EditorToolTip tooltip { get; private set; }
        public MultiNoteDetector multinote { get; private set; }

        public ScriptEditor scriptEditor;

        public Camera Camera;
        public Camera cam => Camera;
        public TimingController timing;
        public HotKeyManager hotkey;

        public GameObject SingleNote;
        public GameObject FlickNote;
        public GameObject SlideNote;
        public GameObject GridInfoText;

        [HideInInspector]
        public V2.Chart chart { get; private set; }

        [HideInInspector]
        public V2.TimingGroup group => chart.groups[editor.currentTimingGroup];

        [HideInInspector]
        public MappingParams parameters;

        public UnityEvent onTimingModified { get; } = new UnityEvent();
        public UnityEvent onGridMoved { get; } = new UnityEvent();
        public UnityEvent onGridModifed { get; } = new UnityEvent();
        public UnityEvent onToolSwitched { get; } = new UnityEvent();
        public UnityEvent onAudioLoaded { get; } = new UnityEvent();
        public UnityEvent onChartLoaded { get; } = new UnityEvent();
        public UnityEvent onUserChangeAudioProgress { get; } = new UnityEvent();

        public ChangeEvent<int> onYSnapModified { get; } = new ChangeEvent<int>();
        public UnityEvent onYPosModified { get; } = new UnityEvent();
        public UnityEvent onYFilterSwitched { get; } = new UnityEvent();

        public UnityEvent onSpeedViewSwitched { get; } = new UnityEvent();

        public NoteEvent onNoteCreated { get; } = new NoteEvent();
        public NoteEvent onNoteModified { get; } = new NoteEvent();
        public NoteEvent onNoteYModified { get; } = new NoteEvent();
        public NoteEvent onNoteRemoved { get; } = new NoteEvent();

        public UnityEvent onTimingPointModified { get; } = new UnityEvent();
        public UnityEvent onTimingGroupModified { get; } = new UnityEvent();
        public UnityEvent onTimingGroupSwitched { get; } = new UnityEvent();

        private LinkedList<IEditorCmd> cmdList;
        private LinkedListNode<IEditorCmd> lastCmd;
        private const int MAX_UNDO_COUNT = 256;

        void Awake()
        {
            chart = new V2.Chart();
            tooltip = EditorToolTip.Create(transform);

            // Get parameters
            parameters = SceneLoader.GetParamsOrDefault<MappingParams>();

            // Init object pool
            pool.Init(SingleNote, FlickNote, SlideNote, GridInfoText);

            // Add listeners
            onAudioLoaded.AddListener(RefreshBarCount);
            onTimingModified.AddListener(RefreshBarCount);

            // Initialize commands
            cmdList = new LinkedList<IEditorCmd>();
            lastCmd = cmdList.AddLast(new FailCommand());

            // Initialize ground notes
            multinote = new MultiNoteDetector(this);

            // Load chart and music
            progress.Init();
            MappingInit.Init(this);
        }

        private void LateUpdate()
        {
            pool.PostUpdate();
        }

        public void SwitchTool(EditorTool tool)
        {
            if (editor.tool == tool)
                return;
            editor.tool = tool;
            onToolSwitched.Invoke();
        }

        #region IEditorCmd
        /// <summary>
        /// Commit an editor command.
        /// </summary>
        /// <returns>Whether the commit is successful.</returns>
        public bool Commit(IEditorCmd cmd)
        {
            if (!cmd.Commit(this))
                return false;
            while (lastCmd.Next != null)
                cmdList.RemoveLast();
            lastCmd = cmdList.AddLast(cmd);
            while (cmdList.Count > MAX_UNDO_COUNT)
                cmdList.Remove(cmdList.First.Next);
            return true;
        }


        /// <summary>
        /// Undo the last editor command.
        /// </summary>
        /// <returns>Whether the rollback is successful.</returns>
        public bool Rollback()
        {
            if (!lastCmd.Value.Rollback(this))
                return false;
            lastCmd = lastCmd.Previous;
            return true;
        }


        /// <summary>
        /// Redo the last editor command.
        /// </summary>
        /// <returns>Whether the redo is successful.</returns>
        public bool RollbackRollback()
        {
            if (lastCmd.Next == null || !lastCmd.Next.Value.Commit(this))
                return false;
            lastCmd = lastCmd.Next;
            return true;
        }

        public void Undo() { Rollback(); }
        public void Redo() { RollbackRollback(); }
        #endregion

        #region Note
        public bool CreateNote(V2.Note note, bool allowMultiNote = false)
        {
            var beat = ChartUtility.BeatToFloat(note.beat);
            if (beat < -NoteUtility.EPS)
                return false;

            if (allowMultiNote)
                multinote.Put(note);
            else if (!multinote.TryPut(note))
            {
                messageBannerController.ShowMsg(LogLevel.INFO, "Editor.NoteAtSamePosition".L());
                return false;
            }

            chart.groups[note.group].notes.Add(note);
            onNoteCreated.Invoke(note);
            return true;
        }

        public bool RemoveNote(V2.Note note)
        {
            multinote.Remove(note);

            chart.groups[note.group].notes.Remove(note);
            onNoteRemoved.Invoke(note);
            return true;
        }
        #endregion

        #region Grid
        public void SetGridDivision(int div)
        {
            if (div == editor.gridDivision)
                return;
            editor.gridDivision = div;
            onGridModifed.Invoke();
        }

        public void IncreaseBarHeight(int delta)
        {
            int result = Mathf.Clamp(editor.barHeight + delta, EditorInfo.MIN_BAR_HEIGHT, EditorInfo.MAX_BAR_HEIGHT);
            if (result == editor.barHeight)
                return;
            float beat = editor.scrollPos / editor.barHeight;
            editor.barHeight = result;
            editor.scrollPos = beat * result;
            onGridModifed.Invoke();
        }

        public void SeekGrid(float target, bool force = false)
        {
            if (!force && !progress.canSeek)
                return;
            editor.scrollPos = Mathf.Clamp(target, 0, editor.maxHeight);
            onGridMoved.Invoke();
        }

        public void MoveGrid(float delta, bool force = false)
        {
            SeekGrid(editor.scrollPos + delta, force);
        }

        public void RefreshBarCount()
        {
            if (chart.bpm.Count == 0)
            {
                // Timing not loaded
                return;
            }
            float duration = audioManager.gameBGM.GetLength() / 1000f;
            editor.numBeats = Mathf.CeilToInt(chart.TimeToBeat(duration));
            onGridModifed.Invoke();
        }
        #endregion

        #region FuwaFuwa
        public void SetYDivision(int div)
        {
            if (div == editor.yDivision)
                return;
            int prev = editor.yDivision;
            editor.yDivision = div;
            if (div == 0)
                editor.yPos = 0;
            onYSnapModified.Invoke(prev, div);
        }

        public void SetY(float y)
        {
            if (NoteUtility.Approximately(y, editor.yPos))
                return;
            editor.yPos = y;
            onYPosModified.Invoke();
        }

        #endregion

        #region Chart
        public V2.Chart GetFinalizedChart()
        {
            var ret = new V2.Chart
            {
                difficulty = chart.difficulty,
                level = chart.level,
                offset = chart.offset,
            };
            ret.bpm.AddRange(chart.bpm);
            // Slides of length 1 must be excluded
            var cmds = new CmdGroup();
            for (int i = 0; i < chart.groups.Count; i++)
            {
                var group = chart.groups[i];
                group.notes.Where(note =>
                {
                    var notebase = notes.Find(note) as EditorSlideNote;
                    return notebase != null && notebase.prev == null && notebase.next == null;
                }).ToList().ForEach(note => cmds.Add(new RemoveNoteCmd(note)));
            }
            Commit(cmds);
            chart.groups.ForEach(group =>
            {
                group.notes.Sort((lhs, rhs) =>
                {
                    float lbeat = ChartUtility.BeatToFloat(lhs.beat);
                    float rbeat = ChartUtility.BeatToFloat(rhs.beat);
                    if (Mathf.Approximately(lbeat, rbeat))
                        return 0;
                    return Math.Sign(lbeat - rbeat);
                });
                ret.groups.Add(group);
            });

            return ret;
        }

        public void Save()
        {
            dataLoader.SaveChart(GetFinalizedChart(), chartLoader.header.sid, parameters.difficulty);

            if (!string.IsNullOrEmpty(scriptEditor.Code))
                dataLoader.SaveChartScript(scriptEditor.Code, chartLoader.header.sid, parameters.difficulty);

            messageBannerController.ShowMsg(LogLevel.OK, "Editor.ChartSaved".L());
        }

        public async void Exit()
        {
            if (Blocker.gameObject.activeSelf || messageBox.isActive)
                return;
            progress.Pause();
            int choice = await messageBox.ShowMessage(
                "Editor.Title.Exit".L(),
                "Editor.Prompt.Exit".L(),
                "Cancel".L(),
                "Editor.ExitOption.ExitNoSave".L(),
                "Editor.ExitOption.ExitSave".L());
            if (choice == 0)
            {
                return;
            }
            if (choice == 2)
            {
                Save();
            }
            SceneLoader.Back(null);
        }

        public void AssignTimingGroups(V2.Chart chart)
        {
            for (int i = 0; i < chart.groups.Count; i++)
            {
                chart.groups[i].notes.ForEach(note => note.group = i);
            }
        }

        public void LoadChart()
        {
            V2.Chart raw = chartLoader.chart;
            AssignTimingGroups(raw);
            chart = new V2.Chart
            {
                difficulty = raw.difficulty,
                level = raw.level,
                offset = raw.offset,
            };
            chart.bpm.AddRange(raw.bpm);
            timing.BpmList = chart.bpm;
            for (int i = 0; i < raw.groups.Count; i++)
            {
                var group = new V2.TimingGroup();
                group.points.AddRange(raw.groups[i].points);
                chart.groups.Add(group);
            }
            for (int i = 0; i < raw.groups.Count; i++)
            {
                // We need to separate bpm notes and game notes here since they're handled differently
                var tickStackDic = new Dictionary<int, V2.Note>();
                var idmap = new Dictionary<int, int>();
                foreach (var note in raw.groups[i].notes)
                {
                    if (note.type == NoteType.BPM)
                    {
                        Debug.LogError("BPM notes are not supported in current chart version!");
                        continue;
                    }
                    if (ChartUtility.IsFuwafuwa(note))
                    {
                        // TODO: handle fuwafuwa notes correctly
                    }
                    if (note.tickStack <= 0)
                    {
                        CreateNote(note, true);
                        continue;
                    }
                    // Note is part of a slide
                    int tickStack = note.tickStack;
                    if (tickStackDic.ContainsKey(tickStack))
                    {
                        // slide body
                        if (note.type == NoteType.Single)
                        {
                            Debug.LogWarning(ChartLoader.BeatToString(note.beat) + "Slide with multiple starts. Translated to slide tick.");
                            note.type = NoteType.SlideTick;
                        }
                        note.tickStack = idmap[tickStack];
                        var prev = tickStackDic[tickStack];
                        CreateNote(note, true);
                        notes.ConnectNote(prev, note);
                    }
                    else
                    {
                        // slide start
                        if (note.type != NoteType.Single)
                        {
                            if (note.type == NoteType.Flick || note.type == NoteType.SlideTickEnd)
                            {
                                Debug.LogWarning(ChartLoader.BeatToString(note.beat) + "Slide without a start. Translated to single note.");
                                note.tickStack = 0;
                                if (note.type != NoteType.Flick)
                                    note.type = NoteType.Single;
                                CreateNote(note, true);
                                continue;
                            }
                            Debug.LogWarning(ChartLoader.BeatToString(note.beat) + "Start of a slide must be 'Single' instead of '" + note.type + "'.");
                            note.type = NoteType.Single;
                        }
                        int id = notes.slideIdPool.RegisterNext();
                        idmap[note.tickStack] = id;
                        note.tickStack = id;
                        CreateNote(note, true);
                    }

                    if (note.type == NoteType.SlideTickEnd || note.type == NoteType.Flick)
                        tickStackDic.Remove(tickStack);
                    else
                        tickStackDic[tickStack] = note;
                }
                if (tickStackDic.Count > 0)
                {
                    foreach (var note in tickStackDic)
                    {
                        note.Value.type = NoteType.SlideTickEnd;
                    }
                    notes.Refresh();
                    Debug.LogWarning("Some slides do not contain a tail. Ignored.");
                }
                onTimingModified.Invoke();
                notes.UnselectAll();
            }

            if (fs.FileExists(dataLoader.GetChartScriptPath(parameters.sid, parameters.difficulty)))
                scriptEditor.Code = fs.GetFile(dataLoader.GetChartScriptPath(parameters.sid, parameters.difficulty)).ReadAsString();

            onChartLoaded.Invoke();
            hotkey.onScroll.AddListener((delta) => MoveGrid(delta * 2));
        }
        #endregion
    }
}
