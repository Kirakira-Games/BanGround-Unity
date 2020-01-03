using UnityEngine;
public abstract class NoteBase : MonoBehaviour
{
    public int lane;
    public int time;
    public int judgeTime = -1;
    public int touchId = -1;
    public int syncLane;
    public bool isGray;

    public GameNoteType type;
    public JudgeResult judgeResult = JudgeResult.None;

    protected MeshRenderer mesh;

    private Vector3 _cachedInitPos = Vector3.zero;
    private Vector3 _cachedJudgePos = Vector3.zero;

    public Vector3 initPos { get { return _cachedInitPos == Vector3.zero ? _cachedInitPos = NoteUtility.GetInitPos(lane) : _cachedInitPos; } }
    public Vector3 judgePos { get { return _cachedJudgePos == Vector3.zero ? _cachedJudgePos = NoteUtility.GetJudgePos(lane) : _cachedJudgePos; } }

    protected virtual void Start()
    {
        touchId = -1;
        judgeTime = -1;
        judgeResult = JudgeResult.None;
        transform.position = initPos;
        transform.localScale = new Vector3(NoteUtility.NOTE_SCALE, NoteUtility.NOTE_SCALE, 1) * LiveSetting.noteSize;

        mesh = NoteMesh.Create(gameObject, lane);

        if (syncLane != -1)
        {
            NoteSyncLine.Create(transform, syncLane - lane);
        }

        OnNoteUpdate();
    }

    public virtual void UpdatePosition(int audioTime)
    {
        int timeSub = time - audioTime;

        Vector3 newPos = _cachedInitPos;
        newPos.z = initPos.z - (NoteUtility.NOTE_START_POS - NoteUtility.NOTE_JUDGE_POS) * (1 - (float)timeSub / LiveSetting.NoteScreenTime);
        transform.position = newPos;
    }

    protected virtual void OnDestroy()
    {
        if (touchId != -1)
        {
            NoteController.controller.UnregisterTouch(touchId, gameObject);
        }
    }

    protected virtual void OnNoteUpdateJudge(int audioTime)
    {
        if (audioTime > time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }

    // This method should not be overriden
    public void OnNoteUpdate()
    {
        int audioTime = (int)(Time.time * 1000);
        if (judgeResult == JudgeResult.None)
        {
            UpdatePosition(audioTime);
        }
        if (LiveSetting.autoPlayEnabled)
        {
            if (audioTime >= time - NoteUtility.AUTO_JUDGE_RANGE)
            {
                RealJudge(audioTime, JudgeResult.Perfect, new Touch());
            }
        }
        else
        {
            OnNoteUpdateJudge(audioTime);
        }
    }

    protected JudgeResult TranslateTimeToJudge(int[] judgeRange, int audioTime)
    {
        int diff = Mathf.Abs(time - audioTime);
        for (int i = 0; i < (int)JudgeResult.Miss; i++)
        {
            if (diff <= judgeRange[i])
            {
                return (JudgeResult)i;
            }
        }
        return JudgeResult.None;
    }

    public virtual JudgeResult TryJudge(int audioTime, Touch touch)
    {
        if (judgeTime != -1 || touch.phase != TouchPhase.Began)
        {
            return JudgeResult.None;
        }
        return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, audioTime);
    }

    public virtual void TraceTouch(int audioTime, Touch touch) { }

    public virtual void RealJudge(int audioTime, JudgeResult result, Touch? touch)
    {
        if (judgeResult != JudgeResult.None) return;
        judgeTime = audioTime;
        judgeResult = result;
        NoteController.controller.Judge(gameObject, result, touch);
        Destroy(gameObject);
    }

    public virtual void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        RealJudge(audioTime, result, touch);
    }
}