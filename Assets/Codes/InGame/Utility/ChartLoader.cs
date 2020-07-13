using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

class GameNoteComparer : Comparer<GameNoteData>
{
    public override int Compare(GameNoteData a, GameNoteData b)
    {
        return a.appearTime - b.appearTime;
    }
}

public static class ChartLoader
{
    public static int numNotes;
    public static string BeatToString(int[] beat)
    {
        return "[" + (beat[1] + beat[2] * beat[0]) + "/" + beat[2] + "]";
    }
    public static int Gcd(int a, int b)
    {
        return a == 0 ? b : Gcd(b % a, a);
    }
    public static void NormalizeBeat(int[] beat)
    {
        int gcd = Gcd(beat[1], beat[2]);
        beat[1] /= gcd;
        beat[2] /= gcd;
        beat[1] += beat[0] * beat[2];
        beat[0] = 0;
    }
    public static GameNoteType TranslateNoteType(V2.Note note)
    {
        if (note.tickStack <= 0)
        {
            if (note.type == NoteType.Single)
            {
                return GameNoteType.Single;
            }
            if (note.type == NoteType.Flick)
            {
                return GameNoteType.Flick;
            }
            Debug.LogError(BeatToString(note.beat) + "Cannot recognize NoteType " + note.type + " on single notes.");
            return GameNoteType.None;
        }
        if (note.type == NoteType.Single)
        {
            return GameNoteType.SlideStart;
        }
        else if (note.type == NoteType.SlideTick)
        {
            return GameNoteType.SlideTick;
        }
        else if (note.type == NoteType.SlideTickEnd)
        {
            return GameNoteType.SlideEnd;
        }
        else if (note.type == NoteType.Flick)
        {
            return GameNoteType.SlideEndFlick;
        }
        Debug.LogError(BeatToString(note.beat) + "Cannot recognize NoteType " + note.type + " on slide notes.");
        return GameNoteType.None;
    }

    public static Vector3 GetJudgePosFromRawNote(V2.Note note)
    {
        Vector3 vec = new Vector3(
            NoteUtility.GetXPos(note.lane == -1 ? note.x : note.lane),
            NoteUtility.GetYPos(note.y),
            NoteUtility.NOTE_JUDGE_Z_POS
        );
        return NoteUtility.ProjectVectorToParallelPlane(vec);
    }

    public static bool IsNoteFuwafuwa(GameNoteData note)
    {
        return note.lane == -1 || note.anims.Any(anim => Mathf.Abs(anim.pos.y) > NoteUtility.EPS);
    }

    public static bool IsNoteFuwafuwa(V2.Note note)
    {
        return note.lane == -1 || note.anims.Any(anim => Mathf.Abs(anim.pos.y) > NoteUtility.EPS);
    }

    public static bool IsChartFuwafuwa(List<GameNoteData> notes)
    {
        return notes.Any(note => IsNoteFuwafuwa(note));
    }

    public static bool IsChartFuwafuwa(List<V2.Note> notes)
    {
        return notes.Any(note => IsNoteFuwafuwa(note));
    }

    private static List<GameNoteData> LoadTimingGroup(ChartTiming timing, int groupId, V2.TimingGroup group)
    {
        // AnalyzeNotes
        var notes = group.notes;
        notes.ForEach(note => NormalizeBeat(note.beat));
        timing.LoadTimingGroup(group);
        // Create game notes
        float prevBeat = -1e9f;
        var tickStackTable = new Dictionary<int, GameNoteData>();
        var ret = new List<GameNoteData>();
        foreach (var note in notes)
        {
            if (prevBeat - note.beatf > NoteUtility.EPS)
            {
                Debug.LogError(BeatToString(note.beat) + "Incorrect order of notes!");
            }
            prevBeat = note.beatf;
            if (note.type == NoteType.BPM)
            {
                throw new InvalidDataException("Unexpected note of type BPM");
            }
            numNotes++;
            // Create game note
            GameNoteType type = TranslateNoteType(note);
            GameNoteData gameNote = new GameNoteData
            {
                time = Mathf.RoundToInt(note.time * 1000),
                lane = note.lane,
                pos = GetJudgePosFromRawNote(note),
                type = type,
                isFuwafuwa = IsNoteFuwafuwa(note),
                isGray = type == GameNoteType.Single && note.beat[2] > 2,
                anims = note.anims,
                appearTime = Mathf.RoundToInt(timing.GetAppearTime(note) * 1000),
                timingGroup = groupId
            };

            // Check slide
            if (note.tickStack == -1)
            {
                // Note is a single note or flick
                ret.Add(gameNote);
                continue;
            }
            // Note is part of a slide
            if (!tickStackTable.ContainsKey(note.tickStack))
            {
                if (note.type != NoteType.Single)
                {
                    if (NoteUtility.IsSlideEnd(type))
                    {
                        Debug.LogWarning(BeatToString(note.beat) + "Slide without a start. Translated to single note.");
                        gameNote.type = type == GameNoteType.SlideEnd ? GameNoteType.Single : GameNoteType.Flick;
                        ret.Add(gameNote);
                        continue;
                    }
                    Debug.LogWarning(BeatToString(note.beat) + "Start of a slide must be 'Single' instead of '" + note.type + "'.");
                }
                GameNoteData tmp = new GameNoteData
                {
                    type = GameNoteType.SlideStart,
                    seg = new List<GameNoteData>()
                };
                tickStackTable[note.tickStack] = tmp;
                type = GameNoteType.SlideStart;
                gameNote.type = type;
                ret.Add(tmp);
            }
            GameNoteData tickStack = tickStackTable[note.tickStack];
            tickStack.seg.Add(gameNote);
            if (NoteUtility.IsSlideEnd(type))
            {
                tickStack.ComputeTime();
                tickStackTable.Remove(note.tickStack);
            }
        }
        if (tickStackTable.Count > 0)
        {
            foreach (var i in tickStackTable)
            {
                i.Value.ComputeTime();
            }
            Debug.LogError("Some slides do not contain a tail. Ignored.");
        }
        return ret;
    }
    
    public static GameTimingGroup ToGameTimingGroup(V2.TimingGroup group)
    {
        return new GameTimingGroup
        {
            points = group.points
        };
    }

    public static GameChartData LoadChart(V2.Chart chart)
    {
        numNotes = 0;
        var timing = new ChartTiming(chart.bpm, chart.offset);
        List<GameNoteData> gameNotes = new List<GameNoteData>();
        for (int i = 0; i < chart.groups.Count; i++)
        {
            LoadTimingGroup(timing, i, chart.groups[i]).ForEach(note => gameNotes.Add(note));
        }

        // Sort notes by animation order
        gameNotes.Sort(new GameNoteComparer());

        return new GameChartData
        {
            isFuwafuwa = IsChartFuwafuwa(gameNotes),
            numNotes = numNotes,
            notes = gameNotes,
            groups = chart.groups.Select(x => ToGameTimingGroup(x)).ToList(),
            bpm = chart.bpm
        };
    }
}
