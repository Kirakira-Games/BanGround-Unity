﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Events;

namespace BGEditor
{
    public class NoteEvent : UnityEvent<Note> { }

    public class ChartCore : MonoBehaviour
    {
        public static ChartCore Instance;

        public GridController grid;
        public EditNoteController notes;
        public TimingController timing;
        public AudioProgressController progress;
        public Camera cam;

        public GameObject SingleNote;
        public GameObject FlickNote;
        public GameObject SlideNote;

        [HideInInspector]
        public Chart chart { get; private set; }

        [HideInInspector]
        public EditorInfo editor { get; private set; }

        [HideInInspector]
        public ObjectPool pool { get; private set; }

        [HideInInspector]
        public Dictionary<NotePosition, int> groundNotes;

        public UnityEvent onTimingModified;
        public UnityEvent onGridMoved;
        public UnityEvent onGridModifed;
        public UnityEvent onToolSwitched;
        public UnityEvent onAudioLoaded;
        public NoteEvent onNoteCreated;
        public NoteEvent onNoteModified;
        public NoteEvent onNoteRemoved;

        private LinkedList<IEditorCmd> cmdList;
        private LinkedListNode<IEditorCmd> lastCmd;
        private const int MAX_UNDO_COUNT = 100;

        void Awake()
        {
            Instance = this;
            chart = new Chart();
            timing.Init();

            // Initialize events
            onTimingModified = new UnityEvent();
            onGridMoved = new UnityEvent();
            onGridModifed = new UnityEvent();
            onToolSwitched = new UnityEvent();
            onAudioLoaded = new UnityEvent();
            onNoteCreated = new NoteEvent();
            onNoteModified = new NoteEvent();
            onNoteRemoved = new NoteEvent();

            // Add listeners
            onAudioLoaded.AddListener(RefreshBarCount);
            onTimingModified.AddListener(RefreshBarCount);

            // Initialize commands
            cmdList = new LinkedList<IEditorCmd>();
            lastCmd = cmdList.AddLast(new FailCommand());

            // Initialize ground notes
            groundNotes = new Dictionary<NotePosition, int>();

            // Initialize editor info
            editor = new EditorInfo();

            // Initialize object pool
            pool = new ObjectPool();

            // Load chart and music
            MappingInit.Init(this);
        }

        private void LateUpdate()
        {
            pool.PostUpdate();
        }

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

        public bool CreateNote(Note note, bool allowMultiNote = false)
        {
            var beat = ChartUtility.BeatToFloat(note.beat);
            if (beat < -NoteUtility.EPS)
                return false;
            if (!ChartUtility.IsFuwafuwa(note))
            {
                var pos = ChartUtility.GetPosition(note);
                if (!groundNotes.ContainsKey(pos))
                    groundNotes.Add(pos, 0);
                if (groundNotes[pos] > 0 && !allowMultiNote)
                {
                    Debug.Log(pos + " was a duplicate.");
                    return false;
                }
                groundNotes[pos]++;
            }
            chart.notes.Add(note);
            onNoteCreated.Invoke(note);
            return true;
        }

        public bool RemoveNote(Note note)
        {
            if (!ChartUtility.IsFuwafuwa(note))
            {
                var pos = ChartUtility.GetPosition(note);
                Debug.Assert(groundNotes.ContainsKey(pos));
                if (groundNotes[pos] <= 0)
                    return false;
                groundNotes[pos]--;
            }
            chart.notes.Remove(note);
            onNoteRemoved.Invoke(note);
            return true;
        }

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

        public void SwitchTool(EditorTool tool)
        {
            if (editor.tool == tool)
                return;
            editor.tool = tool;
            onToolSwitched.Invoke();
        }

        public void RefreshBarCount()
        {
            float duration = AudioManager.Instance.gameBGM.GetLength() / 1000f;
            editor.numBars = Mathf.CeilToInt(timing.TimeToBeat(duration));
            onGridModifed.Invoke();
        }

        public Chart GetFinalizedChart()
        {
            Chart ret = new Chart();
            ret.Difficulty = chart.Difficulty;
            ret.level = chart.level;
            ret.offset = chart.offset;
            // Slides of length 1 must be excluded
            var cmds = new CmdGroup();
            chart.notes.Where(note =>
            {
                var notebase = notes.Find(note) as EditorSlideNote;
                return notebase != null && notebase.prev == null && notebase.next == null;
            }).ToList().ForEach(note => cmds.Add(new RemoveNoteCmd(note)));
            Commit(cmds);

            ret.notes = chart.notes.ToList();
            ret.notes.AddRange(timing.BpmList);
            ret.notes.Sort((lhs, rhs) =>
            {
                float lbeat = ChartUtility.BeatToFloat(lhs.beat);
                float rbeat = ChartUtility.BeatToFloat(rhs.beat);
                if (Mathf.Approximately(lbeat, rbeat))
                    return 0;
                return Math.Sign(lbeat - rbeat);
            });
            return ret;
        }

        public void Save()
        {
            DataLoader.SaveChart(GetFinalizedChart(), LiveSetting.CurrentHeader.sid, (Difficulty) LiveSetting.actualDifficulty);
        }

        public async void Exit()
        {
            progress.Pause();
            if (await MessageBox.ShowMessage("Exit", "Save before exit?"))
            {
                Save();
            }
            SceneLoader.LoadScene("Mapping", "Select");
        }

        public void LoadChart(Chart raw)
        {
            chart = new Chart();
            chart.Difficulty = raw.Difficulty;
            chart.level = raw.level;
            chart.offset = raw.offset;
            chart.notes = new List<Note>();
            // We need to separate bpm notes and game notes here since they're handled differently
            timing.BpmList.Clear();
            var tickStackDic = new Dictionary<int, Note>();
            var idmap = new Dictionary<int, int>();
            foreach (var note in raw.notes)
            {
                if (note.type == NoteType.BPM)
                {
                    timing.BpmList.Add(note);
                    continue;
                }
                if (ChartUtility.IsFuwafuwa(note))
                {
                    // TODO: handle fuwafuwa notes correctly
                    note.lane = Mathf.Clamp(Mathf.RoundToInt(note.x), 0, NoteUtility.LANE_COUNT - 1);
                }
                if (note.tickStack == -1)
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
                        Debug.LogWarning(ChartLoader.BeatToString(note.beat) + "Slide without a start. Translated to single note.");
                        note.tickStack = -1;
                        if (note.type != NoteType.Flick)
                            note.type = NoteType.Single;
                        CreateNote(note, true);
                        continue;
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
    }
}
