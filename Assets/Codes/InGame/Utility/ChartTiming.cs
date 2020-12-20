using UnityEngine;
using System.Collections.Generic;
using V2;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine.Rendering;
using Zenject;
using BGEditor;

class IWithTimingComparer : IComparer<IWithTiming>
{
    public int Compare(IWithTiming lhs, IWithTiming rhs)
    {
        if (lhs.beatf == rhs.beatf) return 0;
        return lhs.beatf > rhs.beatf ? 1 : -1;
    }
}

public class ChartTiming
{
    public readonly V2.Chart chart;
    public List<ValuePoint> bpms { get; private set; }
    public TimingGroup timings { get; private set; }
    private float totTime;

    private delegate bool UnknownPredicate(NoteAnim anim);
    private delegate void Lerp(NoteAnim S, NoteAnim T, NoteAnim i);
    public const float Z_MAX = 1.2f;

    public readonly bool isMirror;

    public ChartTiming(V2.Chart chart, int noteScreenTime, bool isMirror)
    {
        this.isMirror = isMirror;
        this.chart = chart;

        totTime = noteScreenTime / 1000f;
        bpms = chart.bpm;
        bpms.ForEach(bpm => bpm.beatf = ChartUtility.BeatToFloat(bpm.beat));
        bpms.Sort(new IWithTimingComparer());
        // Compute time for BPM
        foreach (var bpm in bpms)
        {
            bpm.time = chart.BeatToTime(bpm.beat);
        }
    }

    public void PopulateTimingInfo(IWithTiming timing)
    {
        if (timing == null)
            return;
        if (timing.beat != null)
            timing.beatf = ChartUtility.BeatToFloat(timing.beat);
        timing.time = chart.BeatToTime(timing.beatf);
    }

    public void LoadTimingGroup(TimingGroup group)
    {
        // Populate necessary information
        timings = group;
        timings.points.ForEach(PopulateTimingInfo);
        timings.notes.ForEach(PopulateTimingInfo);
        timings.notes.ForEach(note => note.anims.ForEach(anim => PopulateTimingInfo(anim)));

        // Process animation data
        timings.notes.ForEach(AddAnimation);
    }

    private int GetPrevIndex<T>(List<T> list, float beatf) where T : IWithTiming
    {
        int l = 0, r = list.Count - 1;
        while (r > l)
        {
            int mid = (l + r + 1) >> 1;
            if (list[mid].beatf > beatf)
                r = mid - 1;
            else
                l = mid;
        }
        return l;
    }

    public float GetAppearTime(V2.Note note)
    {
        return note.anims[0].time;
    }

    public List<V2.NoteAnim> GenerateAnimation(List<V2.NoteAnim> raw, V2.Note note)
    {
        var ret = new List<V2.NoteAnim>();
        if (raw == null)
        {
            return ret;
        }
        float totDist = 0;
        float curDist = 0;
        bool isEnd = false;
        foreach (var anim in raw)
        {
            anim.pos.z += curDist;
            curDist = anim.pos.z;
            if (!isEnd)
                totDist = anim.pos.z;
            //Debug.Log("Raw anim: " + AnimToString(anim));
            if (Mathf.Approximately(anim.time, note.time))
                isEnd = true;
        }
        raw.ForEach(anim => anim.pos.z = anim.pos.z + 1 - totDist);
        //Debug.Log("Tot dist=" + totDist);
        bool isStart = raw[0].pos.z <= 0 || raw[0].pos.z >= Z_MAX;
        if (!isStart)
            ret.Add(raw[0]);
        bool reverse = raw[0].pos.z >= Z_MAX;
        for (int i = 0; i < raw.Count - 1; i++)
        {
            var cur = raw[i];
            var nxt = raw[i + 1];
            if (isStart)
            {
                if (reverse ? nxt.pos.z >= Z_MAX : nxt.pos.z <= 0)
                {
                    continue;
                }
                else
                {
                    isStart = false;
                    float ratio = Mathf.InverseLerp(cur.pos.z, nxt.pos.z, reverse ? Z_MAX : 0f);
                    var anim = V2.NoteAnim.LerpUnclamped(cur, nxt, ratio);
                    ret.Add(anim);
                }
            }
            if (!isStart)
            {
                ret.Add(nxt);
            }
        }
        return ret;
    }

    public void GenerateAnimationRawData(V2.NoteAnim S, V2.NoteAnim T, List<V2.NoteAnim> output)
    {
        //Debug.Log("Anim: " + beatStart + " / " + beatEnd);
        /*
        float totDist = 0;
        for (int i = GetPrevIndex(timings.points, S.beatf); i < timings.points.Count; i++)
        {
            var info = timings.points[i];
            if (info.beatf > T.beatf)
                break;
            //Debug.Log("Speed: " + info.value + " / " + info.beat + " / " + anim.speed);
            float timeS = Mathf.Max(S.time, info.time);
            float timeT = (i == timings.points.Count - 1 || T.beatf < timings.points[i + 1].beatf) ?
                T.time : timings.points[i + 1].time;
            //Debug.Log($"timeT = {timeT}, timeS = {timeS}, speed = {info.speed}, totTime = {totTime}");

            totDist += (timeT - timeS) * info.speed / totTime; // Assume constant speed transition here
        }
        */

        float curDist = 0;
        for (int i = GetPrevIndex(timings.points, S.beatf); i < timings.points.Count; i++)
        {
            var info = timings.points[i];
            if (info.beatf > T.beatf)
                break;
            //Debug.Log("Speed: " + info.value + " / " + info.beat + " / " + anim.speed);
            float timeS = Mathf.Max(S.time, info.time);
            float timeT = T.time;
            float beatT = T.beatf;
            V2.NoteAnim anim = T;
            bool isLast = i == timings.points.Count - 1 || T.beatf < timings.points[i + 1].beatf;

            if (!isLast)
            {
                timeT = timings.points[i + 1].time;
                beatT = timings.points[i + 1].beatf;
            }

            float dist = (timeT - timeS) * info.speed / totTime;
            curDist += dist; // Assume constant speed transition here

            if (!isLast)
            {
                anim = new V2.NoteAnim
                {
                    beatf = beatT,
                    pos = TransitionVector.LerpUnclamped(S.pos, T.pos, Mathf.InverseLerp(S.time, T.time, timeT))
                };
            }
            anim.pos.z = dist; // z temporarily stores the distance traveled
            //Debug.Log("Add anim " + anim + " from " + S + " and " + T);

            PopulateTimingInfo(anim);
            output.Add(anim);
        }
    }

    public void AddAnimation(V2.Note data)
    {
        Debug.Assert(data.beat != null);
        // Generate default animation
        var initAnim = new V2.NoteAnim
        {
            beat = new int[] { 0, -100, 1 },
            pos = new TransitionVector(
                data.lane == -1 ? data.x : data.lane,
                data.y
            )
        };
        PopulateTimingInfo(initAnim);

        // Generate judge animation
        var judgeAnim = new V2.NoteAnim
        {
            beat = data.beat,
            pos = new TransitionVector(
                data.lane == -1 ? data.x : data.lane,
                data.y
            )
        };
        PopulateTimingInfo(judgeAnim);

        // Add existing anims - do not allow animation after judge
        data.anims = data.anims.Where(anim => anim.time < judgeAnim.time).ToList();
        data.anims.Insert(0, initAnim);
        data.anims.Add(judgeAnim);

        // The last animation after judge
        float beatEnd = data.beatf;
        while (chart.BeatToTime(beatEnd) - chart.BeatToTime(data.beatf) <= NoteUtility.SLIDE_TICK_JUDGE_RANGE / 1000f)
        {
            beatEnd += 1f;
        }
        var lastAnim = new V2.NoteAnim
        {
            beatf = beatEnd,
            pos = judgeAnim.pos.Copy()
        };
        PopulateTimingInfo(lastAnim);
        data.anims.Add(lastAnim);

        // Compute appear time and animation
        var tmpList = new List<V2.NoteAnim> { data.anims[0] };
        for (int i = 1; i < data.anims.Count; i++)
        {
            GenerateAnimationRawData(data.anims[i - 1], data.anims[i], tmpList);
        }

        data.anims = GenerateAnimation(tmpList, data);

        // Check mirror
        if (isMirror)
        {
            if (data.lane != -1)
                data.lane = NoteUtility.LANE_COUNT - 1 - data.lane;
            else
                data.x = NoteUtility.LANE_COUNT - 1 - data.x;
            data.anims.ForEach(i => i.pos.x.Set(NoteUtility.LANE_COUNT - 1 - i.pos.x));
        }

        // Map real X,Y coordinates in animations
        data.anims.ForEach(anim =>
        {
            anim.pos.x.Set(NoteUtility.GetXPos(anim.pos.x));
            anim.pos.y.Set(NoteUtility.GetYPos(anim.pos.y));
        });
    }
}
