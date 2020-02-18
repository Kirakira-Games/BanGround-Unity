﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
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
    public static string BeatToString(int[] beat)
    {
        return "[" + beat[1] + "/" + beat[2] + "]";
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
    public static GameNoteType TranslateNoteType(Note note)
    {
        if (note.tickStack == -1)
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

    public static float GetFloatingPointBeat(int[] beat)
    {
        return beat[0] + (float)beat[1] / beat[2];
    }

    public static List<GameNoteData> LoadChart(Chart chart)
    {
        List<Note> notes = chart.notes;
        if (notes == null)
        {
            Debug.LogError("No notes found for current chart.");
            return null;
        }
        List<GameNoteData> gameNotes = new List<GameNoteData>();
        var tickStackTable = new Dictionary<int, GameNoteData>();

        // AnalyzeNotes
        foreach(Note note in notes)
        {
            NormalizeBeat(note.beat);
        }
        ChartTiming timing = new ChartTiming();
        timing.AnalyzeNotes(notes, chart.offset);

        // Compute actual time of each note
        float prevBeat = -1e9f;
        // Create game notes
        foreach (Note note in notes)
        {
            float beat = GetFloatingPointBeat(note.beat);
            if (prevBeat - beat > NoteUtility.EPS)
            {
                Debug.LogError(BeatToString(note.beat) + "Incorrect order of notes!");
            }
            prevBeat = beat;
            if (note.type == NoteType.BPM)
            {
                // Note is a timing point
                continue;
            }
            // Create game note
            float time = timing.GetTimeByBeat(beat);
            GameNoteType type = TranslateNoteType(note);
            GameNoteData gameNote = new GameNoteData
            {
                time = Mathf.RoundToInt(time * 1000),
                lane = note.lane,
                type = type,
                isGray = type == GameNoteType.Single && note.beat[2] > 2
            };
            timing.AddAnimation(note, gameNote);

            // Check slide
            if (note.tickStack == -1)
            {
                // Note is a single note or flick
                gameNotes.Add(gameNote);
                continue;
            }
            // Note is part of a slide
            if (!tickStackTable.ContainsKey(note.tickStack))
            {
//#if UNITY_EDITOR
                if (note.type != NoteType.Single)
                {
                    if (NoteUtility.IsSlideEnd(type))
                    {
                        Debug.LogWarning(BeatToString(note.beat) + "Slide without a start. Translated to single note.");
                        gameNote.type = type == GameNoteType.SlideEnd ? GameNoteType.Single : GameNoteType.Flick;
                        gameNotes.Add(gameNote);
                        continue;
                    }
                    Debug.LogWarning(BeatToString(note.beat) + "Start of a slide must be 'Single' instead of '" + note.type + "'.");
                }
//#endif
                GameNoteData tmp = new GameNoteData
                {
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
        /*
        gameNotes = new List<GameNoteData>
        {
            new GameNoteData
            {
                time = 1000,
                type = GameNoteType.SlideStart,
                seg = new List<GameNoteData>()
                {
                    new GameNoteData
                    {
                        time = 1000,
                        type = GameNoteType.SlideStart,
                        lane = 0
                    },
                    new GameNoteData
                    {
                        time = 2000,
                        type = GameNoteType.SlideTick,
                        lane = 1
                    },
                    new GameNoteData
                    {
                        time = 3000,
                        type = GameNoteType.SlideTick,
                        lane = 2
                    },
                    new GameNoteData
                    {
                        time = 4000,
                        type = GameNoteType.SlideEnd,
                        lane = 3
                    }
                }
            }
        };
        */
        // Sort notes by animation order
        gameNotes.Sort(new GameNoteComparer());

        return gameNotes;
    }
}
