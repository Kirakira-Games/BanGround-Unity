using UnityEngine;
using System.Collections.Generic;

public abstract class NoteBase : MonoBehaviour, KirakiraTracer
{
    public int time;
    public int lane;
    public int judgeTime;
    public int touchId;
    public bool isGray;
    public bool isFuwafuwa => lane == -1;
    public bool isTracingOrJudged => judgeTime != int.MinValue;
    public bool isDestroyed;
    public bool inJudgeQueue;
    public GameNoteAnim[] anims;
    private int animsHead;

    public GameNoteType type;
    public NoteSyncLine syncLine;
    public JudgeResult judgeResult;

    public Vector3 initPos;
    public Vector3 judgePos;

    public virtual void InitNote()
    {
        NoteMesh.CreateMesh(gameObject);
    }

    public virtual void ResetNote(GameNoteData data)
    {
        touchId = -1;
        animsHead = 0;
        judgeTime = int.MinValue;
        judgeResult = JudgeResult.None;
        transform.position = initPos;
        inJudgeQueue = true;
        isDestroyed = false;
        
        time = data.time;
        lane = data.lane;
        type = data.type;
        isGray = LiveSetting.grayNoteEnabled ? data.isGray : false;
        anims = data.anims.ToArray();

        initPos = anims[0].S.p;
        judgePos = data.pos;

        NoteMesh.Reset(gameObject);
    }

    public virtual void UpdatePosition(int audioTime)
    {
        while (animsHead < anims.Length - 1 && audioTime > anims[animsHead].T.t)
            animsHead++;
        while (animsHead > 0 && audioTime < anims[animsHead].S.t)
            animsHead--;

        // Compute ratio of current animation
        var anim = anims[animsHead];
        int timeSub = audioTime - anim.S.t;
        float ratio = (float)timeSub / (anim.T.t - anim.S.t);
        float pos = Mathf.Lerp(anim.S.p.z, anim.T.p.z, ratio);
        //Debug.Log(anim + ", head = " + animsHead + ", ratio = " + ratio);

        // Update position
        Vector3 newPos = Vector3.Lerp(anim.S.p, anim.T.p, ratio);
        if (LiveSetting.bangPerspective)
        {
            pos = NoteUtility.GetBangPerspective(pos);
        }
        newPos.z = Mathf.LerpUnclamped(NoteUtility.NOTE_START_Z_POS, NoteUtility.NOTE_JUDGE_Z_POS, pos);
        newPos = NoteUtility.ProjectVectorToParallelPlane(newPos);
        transform.position = newPos;
    }

    public virtual void OnNoteDestroy()
    {
        if (touchId != -1)
        {
            TouchManager.instance.UnregisterTouch(touchId, this);
        }
    }

    protected virtual void OnNoteUpdateJudge()
    {
        if (NoteController.judgeTime > time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad])
        {
            RealJudge(null, JudgeResult.Miss);
        }
    }

    // This method should not be overriden
    public void OnNoteUpdate()
    {
        if (judgeResult == JudgeResult.None)
        {
            UpdatePosition(NoteController.audioTime);
        }
        if (LiveSetting.autoPlayEnabled)
        {
            if (NoteController.audioTime >= time - NoteUtility.AUTO_JUDGE_RANGE)
            {
                RealJudge(null, JudgeResult.Perfect);
            }
        }
        else
        {
            OnNoteUpdateJudge();
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

    public virtual JudgeResult TryJudge(KirakiraTouch touch)
    {
        if (isTracingOrJudged || touch.current.phase != KirakiraTouchPhase.BEGAN)
        {
            return JudgeResult.None;
        }
        return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, touch.current.time);
    }

    public virtual void TraceTouch(int audioTime, Touch touch) { }

    public virtual void RealJudge(KirakiraTouch touch, JudgeResult result)
    {
        if (judgeResult != JudgeResult.None) return;
        if (!isTracingOrJudged)
            judgeTime = touch == null ? NoteController.judgeTime : touch.current.time;
        judgeResult = result;
        NoteController.instance.Judge(this, result, touch);
        NotePool.instance.DestroyNote(gameObject);
    }

    public virtual void Judge(KirakiraTouch touch, JudgeResult result)
    {
        RealJudge(touch, result);
    }

    public virtual Vector2 GetPosition()
    {
        return judgePos;
    }

    public virtual JudgeResult TryTrace(KirakiraTouch touch)
    {
        return JudgeResult.None;
    }

    public virtual void Trace(KirakiraTouch touch, JudgeResult result)
    {
        if (result != JudgeResult.None)
        {
            RealJudge(touch, result);
        }
    }

    public virtual void Assign(KirakiraTouch touch)
    {
        touchId = touch == null ? -1 : touch.touchId;
    }
}