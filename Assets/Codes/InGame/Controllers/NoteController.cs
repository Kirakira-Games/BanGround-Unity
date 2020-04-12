using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Events;
using AudioProvider;
using JudgeQueue = PriorityQueue<int, NoteBase>;

public class NoteController : MonoBehaviour
{
    public static NoteController instance;
    public static Camera mainCamera;
    public static Vector3 mainForward = new Vector3(0, -0.518944f, 0.8548083f);
    public static int audioTime { get; private set; }
    public static int judgeTime { get; private set; }
    public static int numFuwafuwaNotes;
    public static bool hasFuwafuwaNote => numFuwafuwaNotes > 0;

    private JudgeQueue noteQueue;
    private JudgeQueue[] laneQueue;
    private Dictionary<int, NoteSyncLine> syncTable;

    private GameChartData chart;
    private List<GameNoteData> notes => chart.notes;
    private int noteHead;

    private ISoundEffect[] soundEffects;
    private Animator cameraAnimation;

    private FixBackground background;
    private LaneEffects laneEffects;

    private UnityAction<JudgeResult> onJudge;

    private const int WARM_UP_SECOND = 4;

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

        NotePool.instance.PlayTapEffect(se, position);

        return se;
    }

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

        //粉键震动
        if (LiveSetting.shakeFlick&&(notebase.type == GameNoteType.Flick || notebase.type == GameNoteType.SlideEndFlick)&&result <= JudgeResult.Great)
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
                NotePool.instance.RemoveFromJudgeQueue(obj);
                Q.RemoveFirst();
            }
            else
            {
                break;
            }
        }
    }

    private GameObject CreateNote(GameNoteData gameNote)
    {
        var noteObj = NotePool.instance.GetNote(gameNote.type);
        noteObj.transform.SetParent(transform);
        NoteBase note = noteObj.GetComponent<NoteBase>();
        note.ResetNote(gameNote);
        if (note.judgeFuwafuwa)
        {
            noteQueue.Push(note.time, note);
        }
        else
        {
            laneQueue[note.lane].Push(note.time, note);
        }
        // Add sync line
        if (!syncTable.ContainsKey(note.time) || syncTable[note.time] == null)
        {
            GameObject syncLineObj = new GameObject("syncLine");
            syncLineObj.layer = 8;
            syncLineObj.transform.SetParent(transform);
            syncTable.Remove(note.time);
            syncTable.Add(note.time, syncLineObj.AddComponent<NoteSyncLine>());
        }
        syncTable[note.time].AddNote(note);

        return noteObj;
    }

    public GameObject CreateSlide(List<GameNoteData> notes)
    {
        GameObject obj = NotePool.instance.GetSlide();
        obj.transform.SetParent(transform);
        Slide slide = obj.GetComponent<Slide>();
        slide.InitSlide();
        foreach (GameNoteData note in notes)
        {
            slide.AddNote(CreateNote(note).GetComponent<NoteBase>());
        }
        slide.FinalizeSlide();
        return obj;
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
        if (!UIManager.BitingTheDust)
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
            ret = OnTouch(noteQueue, touch);
            if (ret != null && (noteToJudge == null || noteToJudge.time > ret.time))
            {
                noteToJudge = ret;
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

    private void UpdateNotes(int audioTime)
    {
        while (noteHead < notes.Count)
        {
            GameNoteData note = notes[noteHead];
            if (audioTime <= note.appearTime) break;
            if (note.type == GameNoteType.SlideStart)
            {
                CreateSlide(note.seg);
            }
            else
            {
                CreateNote(note);
            }
            noteHead++;
        }
    }

    void Start()
    {
        instance = this;

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
        int sid = LiveSetting.CurrentHeader.sid;
        chart = ChartLoader.LoadChart(DataLoader.LoadChart(sid, (Difficulty)LiveSetting.actualDifficulty));
        NotePool.instance.Init(notes);
        noteHead = 0;

        // Compute number of notes
        ComboManager.manager.Init(chart.numNotes);

        // Check AutoPlay
        if (LiveSetting.autoPlayEnabled)
        {
            (TouchManager.provider as AutoPlayTouchProvider).Init(notes);
        }

        // Sound effects
        soundEffects = new ISoundEffect[5]
        {
            AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), LiveSetting.seStyle) +"/perfect.wav").bytes),
            AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), LiveSetting.seStyle) +"/great.wav").bytes),
            AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), LiveSetting.seStyle) +"/empty.wav").bytes),
            AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), LiveSetting.seStyle) +"/empty.wav").bytes),
            AudioManager.Instance.PrecacheInGameSE(Resources.Load<TextAsset>("SoundEffects/" + System.Enum.GetName(typeof(SEStyle), LiveSetting.seStyle) +"/flick.wav").bytes)
        };

        AudioManager.Instance.DelayPlayInGameBGM(KiraFilesystem.Instance.Read(DataLoader.GetMusicPath(LiveSetting.CurrentHeader.mid)), WARM_UP_SECOND);

        // Background
        var background = GameObject.Find("InGameBackground").GetComponent<InGameBackground>();
        var (bg, bgtype) = DataLoader.GetBackgroundPath(sid, false);
        if(bgtype == 1)
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
        laneEffects.Init(chart.bpm, chart.speed);

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
                        shutdown = true;
                        AudioManager.Instance.isInGame = false;
                        AudioManager.Instance.StopBGM();
                        GameObject.Find("UIManager").GetComponent<UIManager>().OnAudioFinish(true);
                    }
                });

            else if (mod is PerfectMod) 
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect)
                    {
                        shutdown = true;
                        AudioManager.Instance.isInGame = false;
                        AudioManager.Instance.StopBGM();
                        GameObject.Find("UIManager").GetComponent<UIManager>().OnAudioFinish(true);
                    }
                });
        }
    }

    bool shutdown = false;

    void Update()
    {
        if (SceneLoader.Loading || shutdown || Time.timeScale == 0) return;

        audioTime = AudioTimelineSync.instance.GetTimeInMs() + AudioTimelineSync.RealTimeToBGMTime(LiveSetting.audioOffset);
        judgeTime = audioTime - LiveSetting.judgeOffset;

        // Create notes
        UpdateNotes(audioTime);
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            UpdateLane(laneQueue[i]);
        }
        UpdateLane(noteQueue);

        // Trigger touch event
        TouchManager.instance.OnUpdate();

        // Update each note child
        var noteBase = transform.GetComponentsInChildren<NoteBase>();
        var slide = transform.GetComponentsInChildren<Slide>();
        var noteSyncLine = transform.GetComponentsInChildren<NoteSyncLine>();

        Profiler.BeginSample("OnNoteUpdate");
        for (int i = 0; i < noteBase.Length; i++)
        {
            noteBase[i].OnNoteUpdate();
        }
        Profiler.EndSample();

        for (int i = 0; i < slide.Length; i++)
        {
            slide[i].OnSlideUpdate();
        }

        for (int i = 0; i < noteSyncLine.Length; i++)
        {
            noteSyncLine[i].OnSyncLineUpdate();
        }

        // Update lane effects
        laneEffects.UpdateLaneEffects(audioTime);
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