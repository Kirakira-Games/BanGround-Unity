using UnityEngine;
using System.Collections.Generic;

public abstract class NoteBase : MonoBehaviour, KirakiraTracer
{
    public int time;
    public int lane;
    public int judgeTime;
    public int touchId;
    public bool judgeFuwafuwa => lane == -1;
    public bool displayFuwafuwa;
    public bool isTracingOrJudged => judgeTime != int.MinValue;
    public bool isDestroyed;
    public bool inJudgeQueue;
    public List<V2.NoteAnim> anims;
    private int animsHead;

    public GameNoteType type;
    public NoteSyncLine syncLine;
    public NoteMesh noteMesh;
    public JudgeResult judgeResult;
    public TimingGroupController timingGroup;

    public Vector3 initPos;
    public Vector3 judgePos;

    public virtual void InitNote()
    {
        noteMesh = gameObject.AddComponent<NoteMesh>();
    }

    static KVarRef r_graynote = new KVarRef("r_graynote");
    static KVarRef r_notesize = new KVarRef("r_notesize");

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
        displayFuwafuwa = data.isFuwafuwa;
        anims = data.anims;

        initPos = anims[0].pos;
        judgePos = data.pos;
        gameObject.layer = displayFuwafuwa ? 9 : 8;

        if (displayFuwafuwa)
        {
            NoteController.numFuwafuwaNotes++;
        }

        transform.localScale = Vector3.one * NoteUtility.NOTE_SCALE * r_notesize;

        // Setup material property block
        noteMesh.meshRenderer.SetPropertyBlock(timingGroup.GetMaterialPropertyBlock(type, r_graynote && data.isGray));
    }

    static KVarRef r_bang_perspect = new KVarRef("r_bang_perspect");

    public virtual void UpdatePosition()
    {
        while (animsHead < anims.Count - 1 && NoteController.audioTimef > anims[animsHead + 1].time)
            animsHead++;
        while (animsHead > 0 && NoteController.audioTimef < anims[animsHead].time)
            animsHead--;

        // Compute ratio of current animation
        var anim = anims[animsHead];
        Vector3 newpos;
        if (animsHead == anims.Count - 1)
        {
            newpos = anim.pos;
        }
        else
        {
            var next = anims[animsHead + 1];
            float ratio = Mathf.InverseLerp(anim.time, next.time, NoteController.audioTimef);
            newpos = TransitionVector.Lerp(anim.pos, next.pos, ratio);
        }
        if (r_bang_perspect)
        {
            // Z-pos specialization
            newpos.z = NoteUtility.GetBangPerspective(newpos.z);
        }
        newpos.z = Mathf.LerpUnclamped(NoteUtility.NOTE_START_Z_POS, NoteUtility.NOTE_JUDGE_Z_POS, newpos.z);
        newpos = NoteUtility.ProjectVectorToParallelPlane(newpos);
        transform.position = newpos;

        // update from timing group
    }

    public virtual void OnNoteDestroy()
    {
        if (touchId != -1)
        {
            TouchManager.instance.UnregisterTouch(touchId, this);
        }
        if (displayFuwafuwa)
        {
            NoteController.numFuwafuwaNotes--;
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
            UpdatePosition();
        }
        OnNoteUpdateJudge();
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
        if (isTracingOrJudged || touch.current.phase != KirakiraTouchPhase.Began)
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