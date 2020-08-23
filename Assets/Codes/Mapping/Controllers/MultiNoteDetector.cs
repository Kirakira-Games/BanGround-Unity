using UnityEngine;
using System.Collections.Generic;
using BGEditor;

public class NotePosition
{
    public readonly int x;
    public readonly int y;
    public readonly int beat;

    public NotePosition(V2.Note note)
    {
        x = note.lane == -1 ? Mathf.RoundToInt(note.x * 1000) : note.lane * 1000;
        y = Mathf.RoundToInt(note.y * 1000);
        beat = Mathf.RoundToInt(ChartUtility.BeatToFloat(note.beat) * 1000);
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ beat.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (!(obj is NotePosition rhs))
            return false;
        return x == rhs.x && y == rhs.y && beat == rhs.beat;
    }

    public override string ToString()
    {
        return $"x={x}, y={y}, beat={beat}";
    }
}

public class MultiNoteDetector
{
    private Dictionary<V2.Note, NotePosition> NoteToPosition = new Dictionary<V2.Note, NotePosition>();
    private Dictionary<NotePosition, int> PositionCount = new Dictionary<NotePosition, int>();

    public MultiNoteDetector(IChartCore core)
    {
        core.onNoteYModified.AddListener(OnNoteModified);
    }

    public void Put(V2.Note note)
    {
        var pos = new NotePosition(note);
        if (NoteToPosition.ContainsKey(note))
            NoteToPosition[note] = pos;
        else
            NoteToPosition.Add(note, pos);
        if (!PositionCount.ContainsKey(pos))
            PositionCount.Add(pos, 1);
        else
            PositionCount[pos]++;
    }

    public bool TryPut(V2.Note note)
    {
        var pos = new NotePosition(note);
        if (PositionCount.ContainsKey(pos) && PositionCount[pos] > 0)
            return false;
        Put(note);
        return true;
    }

    public void Remove(V2.Note note)
    {
        var pos = NoteToPosition[note];
        PositionCount[pos]--;
        NoteToPosition.Remove(note);
    }

    private void OnNoteModified(V2.Note note)
    {
        var pos = NoteToPosition[note];
        PositionCount[pos]--;
        Put(note);
    }
}
