using UnityEngine;
using System.Collections.Generic;

class BeatInfoComparer : IComparer<GameBeatInfo>
{
    public int Compare(GameBeatInfo lhs, GameBeatInfo rhs)
    {
        if (lhs.beat == rhs.beat) return 0;
        return lhs.beat > rhs.beat ? 1 : -1;
    }
}

public class ChartTiming
{
    public List<GameBeatInfo> BPMInfo { get; }
    public List<GameBeatInfo> SpeedInfo { get; }
    private float totTime;

    private delegate bool UnknownPredicate(NoteAnim anim);
    private delegate void Lerp(NoteAnim S, NoteAnim T, NoteAnim i);

    public ChartTiming()
    {
        BPMInfo = new List<GameBeatInfo>();
        SpeedInfo = new List<GameBeatInfo>();
        totTime = LiveSetting.NoteScreenTime / 1000f;
    }

    private int GetPrevIndex(List<GameBeatInfo> list, float beat)
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
            if (anim.S.t - note.time >= -1) break;
            totDist += anim.T.p.z;
        }
        //Debug.Log("Tot dist=" + totDist);
        bool isStart = true;
        foreach (var anim in raw)
        {
            anim.S.p.z = 1 - totDist + curDist;
            curDist += anim.T.p.z;
            anim.T.p.z += anim.S.p.z;
            if (isStart)
            {
                if (anim.S.p.z <= 0 && anim.T.p.z <= 0)
                {
                    continue;
                }
                else
                {
                    isStart = false;
                    if (anim.S.p.z <= 0)
                    {
                        float incRatio = (0 - anim.S.p.z) / (anim.T.p.z - anim.S.p.z);
                        anim.S.p.z = 0;
                        anim.S.t += Mathf.RoundToInt((anim.T.t - anim.S.t) * incRatio);
                    }
                }
            }
            Debug.Log("Add anim: " + anim);
            if (anim.S.t < anim.T.t)
                output.Add(anim);
        }
    }

    public void GenerateAnimationRawData(NoteAnim anim, float beatStart, float beatEnd, float nextlane, float nextY, List<GameNoteAnim> output)
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

            float ratioS = (timeS - timeStart) / (timeEnd - timeStart);
            float ratioT = (timeT - timeStart) / (timeEnd - timeStart);
            var newAnim = new GameNoteAnim
            {
                S = new GameNoteAnimState
                {
                    t = Mathf.RoundToInt(timeS * 1000),
                    p = new Vector3(
                        NoteUtility.GetXPos(Mathf.Lerp(anim.lane, nextlane, ratioS)),
                        NoteUtility.GetYPos(Mathf.Lerp(anim.y, nextY, ratioS)),
                        0)
                },
                T = new GameNoteAnimState
                {
                    t = Mathf.RoundToInt(timeT * 1000),
                    p = new Vector3(
                        NoteUtility.GetXPos(Mathf.Lerp(anim.lane, nextlane, ratioT)),
                        NoteUtility.GetYPos(Mathf.Lerp(anim.y, nextY, ratioT)),
                        (timeT - timeS) / totTime * info.value * anim.speed)
                }
            };
            output.Add(newAnim); // T.p.z temporarily stores the distance traveled
        }
    }

    // Fill in unknown properties by interpolation
    private void FillInUnknown(Note data, UnknownPredicate predicate, Lerp lerp, NoteAnim fallback)
    {
        for (int i = 1; i < data.anims.Count; i++)
        {
            if (predicate(data.anims[i]))
            {
                bool suc = false;
                for (int j = i + 1; j < data.anims.Count; j++)
                {
                    if (!predicate(data.anims[j]))
                    {
                        lerp(data.anims[i - 1], data.anims[j], data.anims[i]);
                        suc = true;
                        break;
                    }
                }
                if (!suc)
                {
                    lerp(data.anims[i - 1], fallback, data.anims[i]);
                }
            }
        }
    }

    public void AnalyzeNotes(List<Note> notes, int offset)
    {
        SpeedInfo.Add(new GameBeatInfo
        {
            beat = -100,
            time = -50f,
            value = 1
        });
        foreach (Note note in notes)
        {
            if (note.type != NoteType.BPM)
                continue;
            BPMInfo.Add(new GameBeatInfo
            {
                beat = ChartLoader.GetFloatingPointBeat(note.beat),
                value = note.value
            });
            foreach (NoteAnim anim in note.anims)
            {
                Debug.Assert(!float.IsNaN(anim.speed));
                SpeedInfo.Add(new GameBeatInfo
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
        foreach (GameBeatInfo info in BPMInfo)
        {
            startTime += (info.beat - startDash) * 60 / currentBpm;
            startDash = info.beat;
            currentBpm = info.value;
            info.time = startTime;
        }
        foreach (GameBeatInfo info in SpeedInfo)
        {
            info.time = GetTimeByBeat(info.beat);
        }
    }

    public void AddAnimation(Note data, GameNoteData gameNote)
    {
        // Debug.Log("Add animation");
        gameNote.anims = new List<GameNoteAnim>();
        // Generate default animation
        var initAnim = new NoteAnim
        {
            beat = new int[] { 0, -99, 1 },
            speed = 1,
            lane = data.lane == -1 ? data.x : data.lane,
            y = data.y
        };
        // Update default properties 
        foreach (var i in data.anims)
        {
            if (!float.IsNaN(i.speed))
            {
                initAnim.speed = i.speed;
                break;
            }
        }
        foreach (var i in data.anims)
        {
            if (!float.IsNaN(i.lane))
            {
                initAnim.lane = i.lane;
                break;
            }
        }
        foreach (var i in data.anims)
        {
            if (!float.IsNaN(i.y))
            {
                initAnim.y = i.y;
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
        }

        // Interpolation for unknown properties
        var judgeAnim = new NoteAnim
        {
            beat = data.beat,
            speed = data.anims[data.anims.Count - 1].speed,
            lane = data.lane == -1 ? data.x : data.lane,
            y = data.y
        };
        FillInUnknown(data, anim => float.IsNaN(anim.lane), (S, T, i) =>
        {
            float ts = GetTimeByBeat(S.beat);
            float tt = GetTimeByBeat(T.beat);
            float ti = GetTimeByBeat(i.beat);
            float ratio = Mathf.InverseLerp(ts, tt, ti);
            i.lane = Mathf.Lerp(S.lane, T.lane, ratio);
        }, judgeAnim);
        FillInUnknown(data, anim => float.IsNaN(anim.y), (S, T, i) =>
        {
            float ts = GetTimeByBeat(S.beat);
            float tt = GetTimeByBeat(T.beat);
            float ti = GetTimeByBeat(i.beat);
            float ratio = Mathf.InverseLerp(ts, tt, ti);
            i.y = Mathf.Lerp(S.y, T.y, ratio);
        }, judgeAnim);

        // Compute appear time and animation
        float beatStart, beatEnd;
        List<GameNoteAnim> tmpList = new List<GameNoteAnim>();
        for (int i = 0; i < data.anims.Count; i++)
        {
            var anim = data.anims[i];
            beatStart = ChartLoader.GetFloatingPointBeat(anim.beat);
            beatEnd = ChartLoader.GetFloatingPointBeat(i == data.anims.Count - 1 ?
                data.beat : data.anims[i+1].beat);
            if (beatStart > beatEnd - NoteUtility.EPS)
            {
                Debug.LogError(ChartLoader.BeatToString(data.beat) + "Cannot add animation after judgeTime of a note.");
                break;
            }
            if (i == data.anims.Count - 1)
            {
                GenerateAnimationRawData(anim, beatStart, beatEnd, judgeAnim.lane, data.y, tmpList);
            }
            else
            {
                GenerateAnimationRawData(anim, beatStart, beatEnd, data.anims[i + 1].lane, data.anims[i + 1].y, tmpList);
            }
        }
        beatStart = ChartLoader.GetFloatingPointBeat(data.beat);
        beatEnd = beatStart;
        while (GetTimeByBeat(beatEnd) - GetTimeByBeat(data.beat) <= NoteUtility.SLIDE_TICK_JUDGE_RANGE / 1000f)
        {
            beatEnd += 1f;
        }

        // Judge time should split animation
        GenerateAnimationRawData(judgeAnim, beatStart, beatEnd, judgeAnim.lane, data.y, tmpList);

        GenerateAnimation(tmpList, gameNote.anims, gameNote);

        // Compute appear time for the note
        gameNote.appearTime = gameNote.anims[0].S.t;

        // Check mirror
        if (LiveSetting.mirrowEnabled)
        {
            gameNote.lane = 6 - gameNote.lane;
            gameNote.pos.x *= -1;
            foreach (var i in gameNote.anims)
            {
                i.S.p.x *= -1;
                i.T.p.x *= -1;
            }
        }
    }
}
