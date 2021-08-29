using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Events;
using AudioProvider;
using Cysharp.Threading.Tasks;
using JudgeQueue = SortedList<int, NoteBase>;
using Zenject;
using BanGround;
using BanGround.Scripting;
using BanGround.Scene.Params;
using BanGround.Game.Mods;

public class NoteController : MonoBehaviour, INoteController
{
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IChartLoader chartLoader;
    [Inject]
    private IModManager modManager;
    [Inject]
    private IResourceLoader resourceLoader;
    [Inject]
    private IAudioTimelineSync audioTimelineSync;
    [Inject]
    private IGameStateMachine SM;
    [Inject]
    private IUIManager UI;
    [Inject]
    private IScript chartScript;
    [Inject]
    private IKirakiraTouchProvider touchProvider;

    [Inject(Id = "o_judge")]
    private KVar o_judge;
    [Inject(Id = "o_audio")]
    private KVar o_audio;
    [Inject(Id = "skin_particle")]
    private KVar skin_particle;

    public static Camera mainCamera;
    public static Vector3 mainForward = new Vector3(0, -0.518944f, 0.8548083f);

    public static int audioTime { get; private set; }
    public static int judgeTime { get; private set; }
    public static int looseJudgeTime { get; private set; }
    public static float audioTimef { get; private set; }
    public static float judgeTimef { get; private set; }

    public int numFuwafuwaNotes { get; set; }
    public bool hasFuwafuwaNote => numFuwafuwaNotes > 0;
    public bool isFinished { get; private set; }

    public FuwafuwaLane fuwafuwaLane;
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
    //private LaneEffects laneEffects;

    private UnityAction<JudgeResult> onJudge;

    // Saved instances
    private HashSet<NoteBase> notebases = new HashSet<NoteBase>();
    private HashSet<Slide> slides = new HashSet<Slide>();
    private List<NoteBase> notesToDestroy = new List<NoteBase>();
    private List<Slide> slidesToDestroy = new List<Slide>();

    private const float WARM_UP_SECOND = 4f;

    #region Judge
    private TapEffectType EmitEffect(Vector3 position, JudgeResult result, GameNoteType type)
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
        else if (result == JudgeResult.Bad)
            se = TapEffectType.Bad;

        NotePool.Instance.PlayTapEffect(se, position);

        return se;
    }

    [Inject(Id = "r_shake_flick")]
    KVar r_shake_flick;

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
        if (r_shake_flick && (notebase.type == GameNoteType.Flick || notebase.type == GameNoteType.SlideEndFlick) && result <= JudgeResult.Great)
            cameraAnimation.Play("vibe");

        // Tap effect
        int se = (int)EmitEffect(notebase.judgePos, result, notebase.type);
        LightControl.instance.TriggerLight(notebase.lane, (int)result);

        // Sound effect
        if (notebase.syncLine.PlaySoundEffect(se))
        {
            soundEffects[se].PlayOneShot();
        }

        // Display judge result
        JudgeResultController.instance.DisplayJudgeResult(result);

        // Update life
        LifeController.instance.CaculateLife(result, notebase.type);

        // Update combo
        ComboManager.manager.UpdateComboCountAndScore(result);

        // Update EL
        JudgeResultController.instance.DisplayJudgeOffset(notebase, result);

        if(chartScript.HasOnJudge)
            chartScript.OnJudge(notebase, result);
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

    private int[] GetLanesByTouchState(KirakiraTouchState state)
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
        if (SM.inSimpleState)
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
        slide.InitSlide(this);
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

    [Inject(Id = "r_brightness_long")]
    KVar r_brightness_long;

    [Inject]
    private IFileSystem fs;

    async void Start()
    {
        isFinished = false;
        var parameters = SceneLoader.GetParamsOrDefault<InGameParams>();

        // Main camera
        mainCamera = GameObject.Find("GameMainCamera").GetComponent<Camera>();

        cameraAnimation = GameObject.Find("Cameras").GetComponent<Animator>();

        // Load particle
        ParticleSequence.SetParticlePath(KiraPath.Combine("skin/particle/", skin_particle), fs, resourceLoader);

        // Init JudgeRange
        NoteUtility.Init(mainForward, modManager.SpeedCompensationSum);

        // Init fuwafuwa lane
        numFuwafuwaNotes = 0;
        fuwafuwaLane.Init();

        // Create tables for fast lookup
        syncTable = new Dictionary<int, NoteSyncLine>();

        // Create queue for each lane
        laneQueue = new JudgeQueue[NoteUtility.LANE_COUNT];
        for (int i = 0; i < NoteUtility.LANE_COUNT; i++)
        {
            laneQueue[i] = new JudgeQueue();
        }
        noteQueue = new JudgeQueue();

        // Load chart
        chart = chartLoader.gameChart;
        NotePool.Instance.Init(modManager, notes);
        noteHead = 0;

        // Compute number of notes
        ComboManager.manager.Init(chart.numNotes);

        // Timing groups
        timingGroups = chart.groups.Select(g => new TimingGroupController(g, r_brightness_long)).ToArray();

        // Check AutoPlay
        if (touchProvider is AutoPlayTouchProvider provider)
        {
            provider.Init(notes);
        }

        // Sound effects
        soundEffects = new ISoundEffect[]
        {
            await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("perfect.wav").bytes),
            await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("great.wav").bytes),
            await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("empty.wav").bytes),
            await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("empty.wav").bytes),
            await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("empty.wav").bytes),
            await audioManager.PrecacheInGameSE(resourceLoader.LoadSEResource<TextAsset>("flick.wav").bytes)
        };



        // Game BGM
        float startTime = parameters.seekPosition - audioTimelineSync.RealTimeToBGMTime(
            parameters.skipEntranceAnim ? 1f : WARM_UP_SECOND);
        var audioLoadTask = audioManager.StreamGameBGMTrack(fs.GetFile(dataLoader.GetMusicPath(chartLoader.header.mid)).ReadToEnd())
            .ContinueWith((bgm) =>
            {
                modManager.AttachedMods.ForEach(mod => (mod as AudioMod)?.ApplyMod(bgm));
                audioTimelineSync.AudioSeekPos = parameters.seekPosition;
                audioTimelineSync.Time = startTime;
            });

        // Background
        var background = GameObject.Find("InGameBackground").GetComponent<InGameBackground>();
        var (bg, bgtype) = dataLoader.GetBackgroundPath(parameters.sid, false);
        if (bgtype == 1)
        {
            var videoPath = fs.GetFile(bg).Extract();
            background.SetBackground(videoPath, bgtype);
        }
        else
        {
            background.SetBackground(bg, bgtype);
        }


        //background = GameObject.Find("dokidokiBackground").GetComponent<FixBackground>();
        //background.UpdateBackground(dataLoader.GetBackgroundPath(sid));
        /*
        // Lane Effects
        laneEffects = GameObject.Find("Effects").GetComponent<LaneEffects>();
        laneEffects.Init(chart.groups[0]);
        */
        // Check if adjusting offset
        if (parameters.isOffsetGuide)
        {
            GameObject.Find("infoCanvas").GetComponent<Canvas>().enabled = false;
        }
        else
        {
            GameObject.Find("settingsCanvas").GetComponent<Canvas>().enabled = false;
        }

        chartScript.Init(chartLoader.header.sid, chartLoader.chart.difficulty);

        //Set Play Mod Event
        //audioManager.restart = false;
        onJudge = null;
        foreach (var mod in modManager.AttachedMods)
        {
            if (mod is SuddenDeathMod)
            {
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect && result != JudgeResult.Great)
                    {
                        SM.Transit(SM.Current, GameStateMachine.State.Finished);
                        audioManager.StopBGM();
                        UI.OnAudioFinish(true);
                    }
                });
            }
            else if (mod is PerfectMod)
            {
                onJudge += ((JudgeResult result) =>
                {
                    if (result != JudgeResult.Perfect)
                    {
                        SM.Transit(SM.Current, GameStateMachine.State.Finished);
                        audioManager.StopBGM();
                        GameObject.Find("UIManager").GetComponent<UIManager>().OnAudioFinish(true);
                    }
                });
            }
        }

        // Start playing BGM
        await audioLoadTask;
        audioTimelineSync.Play();
    }

    void Update()
    {
        if (SceneLoader.Loading || SM.Base == GameStateMachine.State.Finished || Time.timeScale == 0) return;

        audioTime = audioTimelineSync.TimeInMs + audioTimelineSync.RealTimeToBGMTime(o_audio);
        judgeTime = audioTime - o_judge;
        looseJudgeTime = judgeTime - 100;
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
        //laneEffects.UpdateLaneEffects();

        // Destroy objects
        slidesToDestroy.ForEach(slide => slides.Remove(slide));
        notesToDestroy.ForEach(note => notebases.Remove(note));

        if (chartScript != null)
        {
            chartScript.OnUpdate(audioTime);

            if (chartScript.HasOnBeat)
                chartScript.OnBeat(chartLoader.chart.TimeToBeat(audioTimef));
        }
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
