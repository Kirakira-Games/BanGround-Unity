using UnityEngine;
using System.Collections.Generic;
using Zenject;

public abstract class NoteBase : MonoBehaviour, KirakiraTracer
{
    private KVar r_graynote;
    private KVar r_notesize;
    private KVar r_bang_perspect;

    public int time;
    public int judgeWindowEnd;
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
    protected IResourceLoader resourceLoader;
    protected INoteController noteController;

    public Vector3 initPos;
    public Vector3 judgePos;
    public virtual Color color => noteMesh.meshRenderer.sharedMaterial.GetColor("_Tint");

    public void Inject(KVar r_graynote, KVar r_notesize, KVar r_bang_perspect)
    {
        this.r_graynote = r_graynote;
        this.r_notesize = r_notesize;
        this.r_bang_perspect = r_bang_perspect;
    }

    public virtual void InitNote(IResourceLoader resourceLoader, INoteController noteController)
    {
        this.resourceLoader = resourceLoader;
        this.noteController = noteController;
        noteMesh = gameObject.AddComponent<NoteMesh>();
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
        judgeWindowEnd = time + NoteUtility.TAP_JUDGE_RANGE[(int)JudgeResult.Bad];
        lane = data.lane;
        type = data.type;
        displayFuwafuwa = data.isFuwafuwa;
        anims = data.anims;

        initPos = anims[0].pos;
        judgePos = NoteUtility.ProjectVectorToParallelPlane(data.pos);
        gameObject.layer = displayFuwafuwa ? 9 : 8;

        if (displayFuwafuwa)
        {
            noteController.numFuwafuwaNotes++;
        }

        transform.localScale = Vector3.one * NoteUtility.NOTE_SCALE * r_notesize;

        // Setup material
        noteMesh.meshRenderer.sharedMaterial = timingGroup.GetMaterial(type, noteMesh.meshRenderer.material, r_graynote && data.isGray);
    }


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
            newpos = TransitionVector.LerpVector(anim.pos, next.pos, ratio);
        }
        if (r_bang_perspect)
        {
            // Z-pos specialization
            newpos.z = NoteUtility.GetBangPerspective(newpos.z);
        }
        newpos.z = Mathf.LerpUnclamped(NoteUtility.NOTE_START_Z_POS, NoteUtility.NOTE_JUDGE_Z_POS, newpos.z);
        newpos.z += NoteUtility.GetDeltaZFromJudgePlane(newpos.y);
        transform.position = newpos;
        noteMesh.OnUpdate();
    }

    public virtual void OnNoteDestroy()
    {
        if (touchId != -1)
        {
            TouchManager.instance.UnregisterTouch(touchId, this);
        }
        if (displayFuwafuwa)
        {
            noteController.numFuwafuwaNotes--;
        }
    }

    protected virtual void OnNoteUpdateJudge()
    {
        // Loose time window
        if (NoteController.looseJudgeTime > judgeWindowEnd)
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
            return JudgeResult.None;
        
        if (touch.current.time > judgeWindowEnd)
            return JudgeResult.Miss;

        return TranslateTimeToJudge(NoteUtility.TAP_JUDGE_RANGE, touch.current.time);
    }

    public virtual void TraceTouch(int audioTime, Touch touch) { }

    public virtual void RealJudge(KirakiraTouch touch, JudgeResult result)
    {
        if (judgeResult != JudgeResult.None) return;
        if (!isTracingOrJudged)
            judgeTime = touch == null ? NoteController.judgeTime : touch.current.time;
        judgeResult = result;
        noteController.Judge(this, result, touch);
        NotePool.Instance.DestroyNote(this);
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