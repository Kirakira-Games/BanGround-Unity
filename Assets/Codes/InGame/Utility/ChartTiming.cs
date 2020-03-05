using UnityEngine;
using System.Collections.Generic;

class BeatInfo
{
    public float beat;
    public float time;
    public float value;
}

class BeatInfoComparer : IComparer<BeatInfo>
{
    public int Compare(BeatInfo lhs, BeatInfo rhs)
    {
        if (lhs.beat == rhs.beat) return 0;
        return lhs.beat > rhs.beat ? 1 : -1;
    }
}

public class ChartTiming
{
    private List<BeatInfo> BPMInfo;
    private List<BeatInfo> SpeedInfo;
    private float totTime;

    public ChartTiming()
    {
        BPMInfo = new List<BeatInfo>();
        SpeedInfo = new List<BeatInfo>();
        totTime = LiveSetting.NoteScreenTime / 1000f;
    }

    private int GetPrevIndex(List<BeatInfo> list, float beat)
    {
        int l = 0, r = list.Count - 1;
        while (r > l)
        {
            int mid = (l + r + 1) >> 1;
            if (list[mid].beat > beat)
                r = mid - 1;
            else
                l = mid;
        }
        return l;
    }

    public float GetTimeByBeat(float beat)
    {
        var info = BPMInfo[GetPrevIndex(BPMInfo, beat)];
        return info.time + (beat - info.beat) * 60 / info.value;
    }

    public float GetTimeByBeat(int[] beat)
    {
        return GetTimeByBeat(ChartLoader.GetFloatingPointBeat(beat));
    }

    private string AnimToString(GameNoteAnim anim)
    {
        return string.Format("[{0},{1}] {2}->{3}", anim.startT, anim.endT, anim.startZ, anim.endZ);
    }

    public void GenerateAnimation(List<GameNoteAnim> raw, List<GameNoteAnim> output, GameNoteData note)
    {
        if (raw.Count == 0)
        {
            return;
        }
        float totDist = 0;
        float curDist = 0;
        foreach (var anim in raw)
        {
            //Debug.Log("Raw anim: " + AnimToString(anim));
            if (anim.startT - note.time >= -1) break;
            totDist += anim.endZ;
        }
        // Compute lane animation
        for (int i = 0; i < raw.Count - 1; i++)
        {
            raw[i].endLane = raw[i + 1].startLane;
        }
        raw[raw.Count - 1].endLane = note.lane;
        //Debug.Log("Tot dist=" + totDist);
        bool isStart = true;
        foreach (var anim in raw)
        {
            anim.startZ = 1 - totDist + curDist;
            curDist += anim.endZ;
            anim.endZ += anim.startZ;
            if (isStart)
            {
                if (anim.startZ <= 0 && anim.endZ <= 0)
                {
                    continue;
                }
                else
                {
                    isStart = false;
                    if (anim.startZ <= 0)
                    {
                        float incRatio = (0 - anim.startZ) / (anim.endZ - anim.startZ);
                        anim.startZ = 0;
                        anim.startT += Mathf.RoundToInt((anim.endT - anim.startT) * incRatio);
                    }
                }
            }
            //Debug.Log("Add anim: " + AnimToString(anim));
            output.Add(anim);
        }
    }

    public void GenerateAnimationRawData(NoteAnim anim, float beatStart, float beatEnd, float nextlane, List<GameNoteAnim> output)
    {
        //Debug.Log("Anim: " + beatStart + " / " + beatEnd);
        float timeStart = GetTimeByBeat(beatStart);
        float timeEnd = GetTimeByBeat(beatEnd);
        for (int i = GetPrevIndex(SpeedInfo, beatStart); i < SpeedInfo.Count; i++)
        {
            var info = SpeedInfo[i];
            if (info.beat >= beatEnd)
                break;
            //Debug.Log("Speed: " + info.value + " / " + info.beat + " / " + anim.speed);
            float timeS = beatStart > info.beat ? timeStart : info.time;
            float timeT = (i == SpeedInfo.Count - 1 || beatEnd < SpeedInfo[i + 1].beat) ?
                timeEnd : SpeedInfo[i + 1].time;
            if (timeT < timeS) continue;

            float ratio = (timeS - timeStart) / (timeEnd - timeStart);
            var newAnim = new GameNoteAnim
            {
                startT = Mathf.RoundToInt(timeS * 1000),
                endT = Mathf.RoundToInt(timeT * 1000),
                endZ = (timeT - timeS) / totTime * info.value * anim.speed,
                startLane = ratio * nextlane + (1 - ratio) * anim.lane
            };
            output.Add(newAnim); // endZ temporarily stores the distance traveled
        }
    }

    public void AnalyzeNotes(List<Note> notes, int offset)
    {
        SpeedInfo.Add(new BeatInfo
        {
            beat = -100,
            time = -50f,
            value = 1
        });
        foreach (Note note in notes)
        {
            if (note.type != NoteType.BPM)
                continue;
            BPMInfo.Add(new BeatInfo
            {
                beat = ChartLoader.GetFloatingPointBeat(note.beat),
                value = note.value
            });
            foreach (NoteAnim anim in note.anims)
            {
                Debug.Assert(!float.IsNaN(anim.speed));
                SpeedInfo.Add(new BeatInfo
                {
                    beat = ChartLoader.GetFloatingPointBeat(anim.beat),
                    value = anim.speed
                });
            }
        }
        // Sort by beat time
        var comparer = new BeatInfoComparer();
        BPMInfo.Sort(comparer);
        SpeedInfo.Sort(comparer);

        // Compute time for BPM
        float currentBpm = 120;
        float startDash = 0;
        float startTime = offset / 1000f;
        foreach (BeatInfo info in BPMInfo)
        {
            startTime += (info.beat - startDash) * 60 / currentBpm;
            startDash = info.beat;
            currentBpm = info.value;
            info.time = startTime;
        }
        foreach (BeatInfo info in SpeedInfo)
        {
            info.time = GetTimeByBeat(info.beat);
        }
    }

    public void AddAnimation(Note data, GameNoteData gameNote)
    {
        gameNote.anims = new List<GameNoteAnim>();
        List<GameNoteAnim> tmpList = new List<GameNoteAnim>();
        // Generate default animation
        var initAnim = new NoteAnim
        {
            beat = new int[] { 0, -99, 1 },
            speed = 1,
            lane = data.lane
        };

        foreach (var i in data.anims)
        {
            if (!float.IsNaN(i.speed))
            {
                initAnim.speed = i.speed;
                break;
            }
            if (!float.IsNaN(i.lane))
            {
                initAnim.lane = i.lane;
                break;
            }
        }
        data.anims.Insert(0, initAnim);

        // Override missing animation properties
        for (int i = 1; i < data.anims.Count; i++)
        {
            // Speed - direct propagate
            if (float.IsNaN(data.anims[i].speed))
            {
                data.anims[i].speed = data.anims[i - 1].speed;
            }
            // Lane - interpolation
            if (float.IsNaN(data.anims[i].lane))
            {
                float t0 = GetTimeByBeat(data.anims[i - 1].beat);
                float ti = GetTimeByBeat(data.anims[i].beat);
                for (int j = i + 1; j < data.anims.Count; j++)
                {
                    if (!float.IsNaN(data.anims[j].lane))
                    {
                        float tj = GetTimeByBeat(data.anims[j].beat);
                        float ratio = (ti - t0) / (tj - t0);
                        data.anims[i].lane = ratio * data.anims[j].lane + (1 - ratio) * data.anims[i - 1].lane;
                    }
                }
                if (float.IsNaN(data.anims[i].lane))
                {
                    float tj = GetTimeByBeat(data.beat);
                    float ratio = (ti - t0) / (tj - t0);
                    data.anims[i].lane = ratio * data.lane + (1 - ratio) * data.anims[i - 1].lane;
                }
            }
        }

        // Compute appear time and animation
        float beatStart, beatEnd;
        NoteAnim anim = null;
        for (int i = 0; i < data.anims.Count; i++)
        {
            anim = data.anims[i];
            beatStart = ChartLoader.GetFloatingPointBeat(anim.beat);
            beatEnd = ChartLoader.GetFloatingPointBeat(i == data.anims.Count - 1 ?
                data.beat : data.anims[i+1].beat);
            if (beatStart > beatEnd - NoteUtility.EPS)
            {
                Debug.LogError(ChartLoader.BeatToString(data.beat) + "Cannot add animation after judgeTime of a note.");
                break;
            }
            GenerateAnimationRawData(anim, beatStart, beatEnd,
                i == data.anims.Count - 1 ? data.lane : data.anims[i + 1].lane,
                tmpList);
        }
        beatStart = ChartLoader.GetFloatingPointBeat(data.beat);
        beatEnd = beatStart;
        while (GetTimeByBeat(beatEnd) - GetTimeByBeat(data.beat) <= NoteUtility.SLIDE_TICK_JUDGE_RANGE / 1000f)
        {
            beatEnd += 1f;
        }
        GenerateAnimationRawData(new NoteAnim
        {
            speed = anim.speed,
            lane = data.lane
        }, beatStart, beatEnd, data.lane, tmpList);
        GenerateAnimation(tmpList, gameNote.anims, gameNote);
        //Debug.Log("Appear time: " + gameNote.appearTime);
    }
}
