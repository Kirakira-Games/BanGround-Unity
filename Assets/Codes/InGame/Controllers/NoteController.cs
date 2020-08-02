using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Events;
using AudioProvider;
using UniRx.Async;
using JudgeQueue = PriorityQueue<int, NoteBase>;

public class NoteController : MonoBehaviour
{
    public static NoteController Instance;
    public static Camera mainCamera;
    public static Vector3 mainForward = new Vector3(0, -0.518944f, 0.8548083f);
    
    public static int audioTime { get; private set; }
    public static int judgeTime { get; private set; }
    public static float audioTimef { get; private set; }
    public static float judgeTimef { get; private set; }

    public static int numFuwafuwaNotes;
    public static bool hasFuwafuwaNote => numFuwafuwaNotes > 0;
    public bool isFinished { get; private set; }

    private JudgeQueue noteQueue;
    private JudgeQueue[] laneQueue;
    private Dictionary<int, NoteSyncLine> syncTable;

    private GameChartData chart;
    private TimingGroupController[] timingGroups;
    private GameNoteData[] notes => chart.notes;
    private int noteHead;

    private ISoundEffect[] soundEffects;
    private Animator cameraAnimation;

    private FixBackground background;
    private LaneEffects laneEffects;

    private UnityAction<JudgeResult> onJudge;

    private ChartScript chartScript;

    // Saved instances
    private HashSet<NoteBase> notebases = new HashSet<NoteBase>();
    private HashSet<Slide> slides = new HashSet<Slide>();
    private List<NoteBase> notesToDestroy = new List<NoteBase>();
    private List<Slide> slidesToDestroy = new List<Slide>();

    private const int WARM_UP_SECOND = 4;

    #region Judge
    public TapEffectType EmitEffect(Vector3 position, JudgeResult result, GameNoteType type)
    {
        if (result == JudgeResult.Miss) return TapEffectType.None;

        TapEffectType se = TapEffectType.Click;

        if (NoteUtility.IsFlick(type))
        {
            if (result <= JudgeResult.Good)
                se = TapEffectType.Flick;
        }
        else if (result == JudgeResult.Perfect)
            se = TapEffectType.Perfect;
        else if (result == JudgeResult.Great)
            se = TapEffectType.Great;
        else if (result == JudgeResult.Good)
            se = TapEffectType.Good;

        NotePool.Instance.PlayTapEffect(se, position);

        return se;
    }

    static KVarRef r_shake_flick = new KVarRef("r_shake_flick");

    // Judge a note as result
    public void Judge(NoteBase notebase, JudgeResult result, KirakiraTouch touch)
    {
        GameObject note = notebase.gameObject;

        onJudge?.Invoke(result);

        if (result == JudgeResult.None)
        {
            Debug.LogWarning("'None' cannot be final judge result. Recognized as 'Miss'.");
            result = JudgeResult.Miss;
        }

        // 粉键震动
        if (r_shake_flick && (notebase.type == GameNoteType.Flick || notebase.type == GameNoteType.SlideEndFlick)&&result <= JudgeResult.Great)
            cameraAnimation.Play("vibe");

        // Tap effect
        int se = (int)EmitEffect(notebase.judgePos, result, notebase.type);
        LightControl.instance.TriggerLight(notebase.lane, (int)result);

        // Sound effect
        if (notebase.syncLine.PlaySoundEffect(se))
        {
            soundEffects[se].PlayOneShot();
        }

        // Update score
        JudgeResultController.instance.DisplayJudgeResult(result);

        // Update life
        LifeController.instance.CaculateLife(result, notebase.type);

        // Update combo
        ComboManager.manager.UpdateCombo(result);

        // Update EL
        JudgeResultController.instance.DisplayJudgeOffset(notebase, (int)result);
    }

    private void UpdateLane(JudgeQueue Q)
    {
        // Remove judged and destroyed notes from queue
        while (!Q.Empty)
        {
            NoteBase obj = Q.Top;
            if (obj.isDestroyed || obj.isTracingOrJudged)
            {
                NotePool.Instance.RemoveFromJudgeQueue(obj);
                Q.RemoveFirst();
            }
            else
            {
                break;
            }
        }
    }

    public static int[] GetLanesByTouchState(KirakiraTouchState state)
    {
        var lanes = new List<int>();
        var dists = new float[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            if (TouchManager.TouchesNote(state, i))
            {
                lanes.Add(i);
                dists[i] = Vector2.Distance(state.pos, NoteUtility.GetJudgePos(i));
            }
        }
        lanes.Sort((int lhs, int rhs) =>
        {
            return dists[lhs].CompareTo(dists[rhs]);
        });
        return lanes.ToArray();
    }

    private NoteBase OnTouchFuwafuwa(JudgeQueue Q, KirakiraTouch touch)
    {
        UpdateLane(Q);
        if (Q.Empty)
            return null;
        // Try to judge the front of the queue
        float minDist = NoteUtility.FUWAFUWA_RADIUS;
        NoteBase ret = null;
        for (var i = Q.FirstV; i != null; i = i.Next)
        {
            NoteBase note = i.Value;
            if (note.time > touch.current.time + NoteUtility.SLIDE_TICK_JUDGE_RANGE ||
                (ret != null && ret.time != note.time))
            {
                break;
            }
            if (NoteUtility.IsSlide(note.type) && (note as SlideNoteBase).isJudging)
            {
                continue;
            }
            float dist = TouchManager.TouchDist(touch.current, note.judgePos);
            if (dist > minDist)
            {
                continue;
            }
            JudgeResult result = note.TryJudge(touch);
            if (result != JudgeResult.None)
            {
                ret = note;
                minDist = dist;
            }
        }
        //Debug.Log($"Match {touch.current} with {(ret == null ? Vector2.negativeInfinity : (Vector2)ret.judgePos)}");
        return ret;
    }

    private NoteBase OnTouch(JudgeQueue Q, KirakiraTouch touch)
    {
        UpdateLane(Q);
        // Try to judge the front of the queue
        for (var i = Q.FirstV; i != null; i = i.Next)
        {
            NoteBase note = i.Value;
            if (note.time > touch.current.time + NoteUtility.SLIDE_TICK_JUDGE_RANGE)
            {
                return null;
            }
            if (NoteUtility.IsSlide(note.type) && (note as SlideNoteBase).isJudging)
            {
                continue;
            }
            if (!TouchManager.TouchesNote(touch.current, note))
            {
                continue;
            }
            JudgeResult result = note.TryJudge(touch);
            if (result != JudgeResult.None)
            {
                return note;
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    public void UpdateTouch(KirakiraTouch touch)
    {
        // Find lanes
        int[] lanes = GetLanesByTouchState(touch.current);

        NoteBase noteToJudge = null;
        if (UIManager.Instance.SM.Count == 1)
        {
            NoteBase ret;
            // Find note to judge - non-fuwafuwa
            foreach (int lane in lanes)
            {
                ret = OnTouch(laneQueue[lane], touch);
                if (ret != null && (noteToJudge == null || noteToJudge.time > ret.time))
                {
                    noteToJudge = ret;
                }
            }
            // Find note to judge - fuwafuwa
            ret = OnTouchFuwafuwa(noteQueue, touch);
            if (ret != null)
            {
                if (noteToJudge == null || noteToJudge.time > ret.time)
                    noteToJudge = ret;
                else if (noteToJudge.time == ret.time &&
                    TouchManager.TouchDist(touch.current, noteToJudge.judgePos)
                    > TouchManager.TouchDist(touch.current, ret.judgePos))
                {
                    noteToJudge = ret;
                }
            }
        }
        // A note to judge is not found
        if (noteToJudge == null)
        {
            if (touch.current.phase == KirakiraTouchPhase.Began && lanes.Length > 0)
            {
                int se = (int)EmitEffect(NoteUtility.GetJudgePos(lanes[0]), JudgeResult.None, GameNoteType.Single);
                soundEffects[se].PlayOneShot();
                LightControl.instance.TriggerLight(lanes[0]);
            }
        }
        else
        {
            noteToJudge.Judge(touch, noteToJudge.TryJudge(touch));
        }
    }
    #endregion

    #region Create
    private NoteBase CreateNote(GameNoteData gameNote)
    {
        var noteObj = NotePool.Instance.GetNote(gameNote.type);
        noteObj.transform.SetParent(transform);
        NoteBase note = noteObj.GetComponent<NoteBase>();
        note.timingGroup = timingGroups[gameNote.timingGroup];
        note.ResetNote(gameNote);
        if (note.judgeFuwafuwa)
        {
            noteQueue.Push(note.time, note);
        }
        else
        {
            laneQueue[note.lane].Push(note.time, note);
        }
        // Add note
        // Add sync line
        if (!syncTable.ContainsKey(note.time))
        {
            var line = NotePool.Instance.GetSyncLine();
            line.transform.SetParent(transform);
            line.ResetLine(note.time);
            syncTable.Add(note.time, line);
        }
        syncTable[note.time].AddNote(note);

        return note;
    }

    public Slide CreateSlide(List<GameNoteData> notes)
    {
        var slide = NotePool.Instance.GetSlide();
        slide.transform.SetParent(transform);
        slide.InitSlide();
        foreach (GameNoteData note in notes)
        {
            slide.AddNote(CreateNote(note));
        }
        slide.FinalizeSlide();
        return slide;
    }
    #endregion

    #region Destroy
    public void OnNoteDestroy(NoteBase note)
    {
        notesToDestroy.Add(note);
    }

    public void OnSlideDestroy(Slide slide)
    {
        slidesToDestroy.Add(slide);
    }

    public void OnSyncLineDestroy(NoteSyncLine line)
    {
        syncTable.Remove(line.time);
    }
    #endregion

    private void UpdateNotes()
    {
        while (noteHead < notes.Length)
        {
            GameNoteData note = notes[noteHead];
            if (audioTime <= note.appearTime) break;
            if (note.type == GameNoteType.SlideStart)
            {
                slides.Add(CreateSlide(note.seg));
            }
            else
            {
                notebases.Add(CreateNote(note));
            }
            noteHead++;
        }
    }

    static KVarRef mod_autoplay = new KVarRef("mod_autoplay");
    static KVarRef cl_sestyle = new KVarRef("cl_sestyle");

    async void Start()
    {
        Instance = this;
        isFinished = false;

        // Main camera
        mainCamera = GameObject.Find("GameMainCamera").GetComponent<Camera>();

        cameraAnimation = GameObject.Find("Cameras").GetComponent<Animator>();

        // Init JudgeRange
        NoteUtility.Init(mainForward);

        // Init fuwafuwa lane
        numFuwafuwaNotes = 0;
        FuwafuwaLane.instance.Init();

        // Create tables for fast lookup
        syncTable = new Dictionary<int, NoteSyncLine>();
        Application.targetFrameRate = 120;

        // Create queue for each lane
        laneQueue = new JudgeQueue[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new JudgeQueue();
        }
        noteQueue = new JudgeQueue();

        // Load chart
        chart = LiveSetting.gameChart;
        NotePool.Instance.Init(notes);
        noteHead = 0;

        // Compute number of notes
        ComboManager.manager.Init(chart.numNotes);

        // Timing groups
        timingGroups = chart.groups.Select(g => new TimingGroupController(g)).ToArray();

        // Check AutoPlay
        if (mod_autoplay)
        {
            (TouchManager.provider as AutoPlayTouchProvider).Init(notes);
        }

        // Sound effects
        soundEffects = new ISoundEffect[5]
        {
            await AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), (SEStyle)cl_sestyle) +"/perfect.wav").bytes),
            await AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), (SEStyle)cl_sestyle) +"/great.wav").bytes),
            await AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), (SEStyle)cl_sestyle) +"/empty.wav").bytes),
            await AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), (SEStyle)cl_sestyle) +"/empty.wav").bytes),
            await AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), (SEStyle)cl_sestyle) +"/flick.wav").bytes)
        };

        // Game BGM
        StartCoroutine(AudioManager.Instance.DelayPlayInGameBGM(KiraFilesystem.Instance.Read(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)), WARM_UP_SECOND).ToCoroutine());

        // Background
        var background = GameObject.Find("InGameBackground").GetComponent<InGameBackground>();
        var (bg, bgtype) = DataLoader.GetBackgroundPath(LiveSetting.CurrentHeader.sid, false);
        if (bgtype == 1)
        {
            var videoPath = KiraFilesystem.Instance.Extract(bg);
            background.SetBackground(videoPath, bgtype);
        }
        else
        {
            background.SetBackground(bg, bgtype);
        }


        //background = GameObject.Find("dokidokiBackground").GetComponent<FixBackground>();
        //background.UpdateBackground(DataLoader.GetBackgroundPath(sid));

        // Lane Effects
        laneEffects = GameObject.Find("Effects").GetComponent<LaneEffects>();
        laneEffects.Init(chart.groups[0]);

        // Check if adjusting offset
        if (LiveSetting.offsetAdjustMode)
        {
            GameObject.Find("infoCanvas").GetComponent<Canvas>().enabled = false;
            LiveSetting.attachedMods.Clear();
        }
        else
        {
            GameObject.Find("settingsCanvas").GetComponent<Canvas>().enabled = false;
        }

        chartScript = new ChartScript(LiveSetting.CurrentHeader.sid, (Difficulty)LiveSetting.actualDifficulty);

        //Set Play Mod Event
        //AudioManager.Instance.restart = false;
        onJudge = null;
        foreach (var mod in LiveSetting.attachedMods)
        {
            if (mod is SuddenDeathMod)
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect && result != JudgeResult.Great)
                    {
                        UIManager.Instance.SM.Transit(UIManager.Instance.SM.Current, GameStateMachine.State.Finished);
                        AudioManager.Instance.StopBGM();
                        UIManager.Instance.OnAudioFinish(true);
                    }
                });

            else if (mod is PerfectMod)
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect)
                    {
                        UIManager.Instance.SM.Transit(UIManager.Instance.SM.Current, GameStateMachine.State.Finished);
                        AudioManager.Instance.StopBGM();
                        GameObject.Find("UIManager").GetComponent<UIManager>().OnAudioFinish(true);
                    }
                });
        }

        // Init notes
        NoteBase.Init();
    }

    static KVarRef o_judge = new KVarRef("o_judge");
    static KVarRef o_audio = new KVarRef("o_audio");

    void Update()
    {
        if (SceneLoader.Loading || UIManager.Instance.SM.Base == GameStateMachine.State.Finished || Time.timeScale == 0) return;

        audioTime = AudioTimelineSync.instance.GetTimeInMs() + AudioTimelineSync.RealTimeToBGMTime(o_audio);
        judgeTime = audioTime - o_judge;
        audioTimef = audioTime / 1000f;
        judgeTimef = judgeTime / 1000f;
        slidesToDestroy.Clear();
        notesToDestroy.Clear();
        //Debug.Log("Audio: " + audioTime);

        // Create notes
        UpdateNotes();
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            UpdateLane(laneQueue[i]);
        }
        UpdateLane(noteQueue);

        // Update timing groups
        foreach (var i in timingGroups)
        {
            i.OnUpdate();
        }

        // Trigger touch event
        TouchManager.instance.OnUpdate();

        // Update each note child
        Profiler.BeginSample("OnNoteUpdate");

        foreach (var i in notebases)
        {
            i.OnNoteUpdate();
        }
        Profiler.EndSample();

        foreach (var i in slides)
        {
            i.OnSlideUpdate();
        }

        // Update sync lines
        var values = syncTable.Values.ToArray();
        foreach (var i in values)
        {
            i.OnSyncLineUpdate();
        }

        // Update isfinished
        isFinished = noteHead >= notes.Length;
        if (noteQueue.Count > 0)
            isFinished = false;
        if (laneQueue.Any(Q => Q.Count > 0))
            isFinished = false;

        // Update lane effects
        laneEffects.UpdateLaneEffects();

        // Destroy objects
        slidesToDestroy.ForEach(slide => slides.Remove(slide));
        notesToDestroy.ForEach(note => notebases.Remove(note));

        chartScript?.OnUpdate(audioTime);
    }

    private void OnDestroy()
    {
        if (soundEffects != null) 
        {
            for (int i = 0; i < soundEffects.Length; i++)
            {
                soundEffects[i].Dispose();
            }
        }
    }
}