using UnityEngine;
using System.Collections.Generic;

public abstract class NoteBase : MonoBehaviour
{
    public int lane;
    public int time;
    public int judgeTime;
    public int touchId;
    public bool isGray;
    public bool isDestroyed;
    public bool inJudgeQueue;
    public GameNoteAnim[] anims;
    private int animsHead;

    public GameNoteType type;
    public NoteSyncLine syncLine;
    public JudgeResult judgeResult;

    public MeshRenderer mesh;

    protected Vector3 _cachedInitPos;
    protected Vector3 _cachedJudgePos;

    public Vector3 initPos { get { return _cachedInitPos == Vector3.zero ? _cachedInitPos = NoteUtility.GetInitPos(lane) : _cachedInitPos; } }
    public Vector3 judgePos { get { return _cachedJudgePos == Vector3.zero ? _cachedJudgePos = NoteUtility.GetJudgePos(lane) : _cachedJudgePos; } }

    public virtual void InitNote()
    {
        touchId = -1;
        animsHead = 0;
        judgeTime = int.MinValue;
        judgeResult = JudgeResult.None;
        _cachedInitPos = Vector3.zero;
        _cachedJudgePos = Vector3.zero;
        transform.position = initPos;
        inJudgeQueue = false;
        isDestroyed = false;
        transform.localScale = new Vector3(NoteUtility.NOTE_SCALE, NoteUtility.NOTE_SCALE, 1) * LiveSetting.noteSize;

        mesh = NoteMesh.Create(gameObject, lane);
    }

    public virtual void UpdatePosition(int audioTime)
    {
        while (animsHead < anims.Length - 1 && audioTime > anims[animsHead].endT)
            animsHead++;
        var anim = anims[animsHead];
        int timeSub = audioTime - anim.startT;
        float ratio = (float)timeSub / (anim.endT - anim.startT);
        float pos = ratio * (anim.endZ - anim.startZ) + anim.startZ;

        Vector3 newPos = _cachedInitPos;
        if (LiveSetting.bangPerspective)
            pos = NoteUtility.GetBangPerspective(pos);
        newPos.z = initPos.z - (NoteUtility.NOTE_START_POS - NoteUtility.NOTE_JUDGE_POS) * pos;
        transform.position = newPos;
    }

    public virtual void OnNoteDestroy()
    {
        if (touchId != -1)
        {
            NoteController.controller.UnregisterTouch(touchId, gameObject);
        }

        int result = (int)judgeResult;
        if (result >= 1 && result <= 3)
        {
            ComboManager.JudgeOffsetResult.Add(time - judgeTime);
            JudgeResultController.instance.DisplayJudgeOffset(time - judgeTime > 0 ? OffsetResult.Early : OffsetResult.Late);
            return;
        }
        JudgeResultController.instance.DisplayJudgeOffset(OffsetResult.None);
    }

    protected virtual void OnNoteUpdateJudge(int audioTime)
    {
        if (audioTime > time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
        {
            RealJudge(audioTime, JudgeResult.Miss, null);
        }
    }

    // This method should not be overriden
    public void OnNoteUpdate(int audioTime)
    {
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
            OnNoteUpdateJudge(audioTime - LiveSetting.judgeOffset);
        }
    }

    protected JudgeResult TranslateTimeToJudge(int[] judgeRange, int audioTime)
    {
        int offset = time - audioTime;
        int diff = Mathf.Abs(offset);
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
        if (judgeTime != int.MinValue || touch.phase != TouchPhase.Began)
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
        NotePool.instance.DestroyNote(gameObject);
    }

    public virtual void Judge(int audioTime, JudgeResult result, Touch? touch)
    {
        RealJudge(audioTime, result, touch);
    }

    public void OnDestroy()
    {
        Debug.Log("note destroyed");
    }
}