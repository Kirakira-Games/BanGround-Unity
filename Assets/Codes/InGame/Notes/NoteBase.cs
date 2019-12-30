using UnityEngine;
public abstract class NoteBase : MonoBehaviour
{
    public int lane;
    public int time;
    public int judgeTime;
    public int touchId;

    public GameNoteType type;

    private NoteSyncLine syncLine;
    protected SpriteRenderer sprite;

    private Vector3 _cachedInitPos = Vector3.zero;

    public Vector3 initPos { get { return _cachedInitPos == Vector3.zero ? _cachedInitPos = NoteUtility.GetInitPos(lane) : _cachedInitPos; } }

    protected virtual void Start()
    {
        touchId = -1;
        judgeTime = -1;
        transform.position = initPos;
        transform.localScale = new Vector3(NoteUtility.NOTE_SCALE, NoteUtility.NOTE_SCALE, 1) * LiveSetting.noteSize;

        sprite = gameObject.AddComponent<SpriteRenderer>();
        sprite.sortingLayerID = SortingLayer.NameToID("Note");

        OnNoteUpdate();
    }

    public virtual void UpdatePosition(int audioTime)
    {
        int timeSub = time - audioTime;

        Vector3 newPos = _cachedInitPos;
        newPos.z = initPos.z - (NoteUtility.NOTE_START_POS - NoteUtility.NOTE_JUDGE_POS) * (1 - (float)timeSub / LiveSetting.NoteScreenTime);
        transform.position = newPos;
    }

    private void OnDestroy()
    {
        if (touchId != -1)
        {
            NoteController.controller.UnregisterTouch(touchId);
        }
    }

    public virtual void OnNoteUpdate()
    {
        int audioTime = (int)(Time.time * 1000);
        UpdatePosition(audioTime);

        if(audioTime > time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
        {
            Judge(audioTime, JudgeResult.Miss, null);
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

    public virtual void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        judgeTime = audioTime;
        NoteController.controller.Judge(gameObject, result, touch);
        Destroy(gameObject);
    }
}