using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

class NoteComparer : Comparer<Note>
{
    public override int Compare(Note a, Note b)
    {
        float t1 = ChartLoader.GetFloatingPointBeat(a.beat);
        float t2 = ChartLoader.GetFloatingPointBeat(b.beat);
        if (Mathf.Abs(t1-t2) <= NoteUtility.EPS)
        {
            return (int)a.type - (int)b.type;
        }
        return t1 < t2 ? -1: 1;
    }
}

public static class ChartLoader
{
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
    public static GameNoteType TranslateNoteType(Note note)
    {
        if (note.tickStack == -1)
        {
            if (note.type == NoteType.Single)
            {
                return GameNoteType.Normal;
            }
            if (note.type == NoteType.Flick)
            {
                return GameNoteType.Flick;
            }
            Debug.LogError("Cannot recognize NoteType " + note.type + " on single notes.");
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
        Debug.LogError("Cannot recognize NoteType " + note.type + " on slide notes.");
        return GameNoteType.None;
    }
    public static Header LoadHeaderFromFile(string path)
    {
        TextAsset headerText = Resources.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<Header>(headerText.text);
    }

    public static Chart LoadChartFromFile(string path)
    {
        TextAsset chartText = Resources.Load<TextAsset>(path);
        return JsonConvert.DeserializeObject<Chart>(chartText.text);
    }

    public static float GetFloatingPointBeat(int[] beat)
    {
        return beat[0] + (float)beat[1] / beat[2];
    }

    public static List<GameNoteData> LoadNotesFromFile(string path)
    {
        Chart chart = LoadChartFromFile(path);
        List<Note> notes = chart.notes;
        if (notes == null)
        {
            Debug.LogError("No notes found for current chart.");
            return null;
        }
        List<GameNoteData> gameNotes = new List<GameNoteData>();
        notes.Sort(new NoteComparer());
        var tickStackTable = new Dictionary<int, GameNoteData>();

        // Compute actual time of each note
        float currentBpm = 120;
        float startDash = 0;
        float startTime = chart.offset / 1000f;
        // Create game notes
        foreach (Note note in notes)
        {
            NormalizeBeat(note.beat);
            float beat = GetFloatingPointBeat(note.beat);
            if (note.type == NoteType.BPM)
            {
                // Note is a timing point
                float beatDuration = 60 / note.value;
                startTime += (beat - startDash) * beatDuration;
                startDash = GetFloatingPointBeat(note.beat);
                currentBpm = note.value;
                continue;
            }
            // Create game note
            float time = startTime + (beat - startDash) * (60 / currentBpm);
            GameNoteType type = TranslateNoteType(note);
            GameNoteData gameNote = new GameNoteData
            {
                time = (int)(time * 1000),
                lane = note.lane,
                type = type,
                isGray = type == GameNoteType.Normal && note.beat[2] <= 2
            };
            if (note.tickStack == -1)
            {
                // Note is a single note or flick
                gameNotes.Add(gameNote);
                continue;
            }
            // Note is part of a slide
            if (!tickStackTable.ContainsKey(note.tickStack))
            {
                if (note.type != NoteType.Single)
                {
                    Debug.LogWarning("Start of a slide must be 'Single' instead of '" + note.type + "'.");
                }
                GameNoteData tmp = new GameNoteData
                {
                    time = (int)(time * 1000),
                    type = GameNoteType.SlideStart,
                    seg = new List<GameNoteData>()
                };
                tickStackTable[note.tickStack] = tmp;
                type = GameNoteType.SlideStart;
                gameNote.type = type;
                gameNotes.Add(tmp);
            }
            GameNoteData tickStack = tickStackTable[note.tickStack] as GameNoteData;
            tickStack.seg.Add(gameNote);
            if (NoteUtility.IsSlideEnd(type))
            {
                tickStackTable.Remove(note.tickStack);
            }
        }
        if (tickStackTable.Count > 0)
        {
            Debug.LogError("Some slides do not contain a tail. Ignored.");
        }
        return gameNotes;
    }
}
