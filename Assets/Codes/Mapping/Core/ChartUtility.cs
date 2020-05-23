namespace BGEditor
{
    public struct NotePosition
    {
        public int lane;
        public int beat;
        public int div;

        public NotePosition(int lane, int beat, int div)
        {
            int g = ChartLoader.Gcd(beat, div);
            beat /= g;
            div /= g;
            this.lane = lane;
            this.beat = beat;
            this.div = div;
        }

        public override bool Equals(object obj)
        {
            return Equals((NotePosition) obj);
        }

        public bool Equals(NotePosition rhs)
        {
            return lane == rhs.lane && beat == rhs.beat && div == rhs.div;
        }

        public override int GetHashCode()
        {
            return (beat << 16) ^ (div << 4) ^ (lane + 1);
        }

        public static bool operator == (NotePosition lhs, NotePosition rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator != (NotePosition lhs, NotePosition rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override string ToString()
        {
            return $"{lane}:{beat}/{div}";
        }
    };

    public static class ChartUtility
    {
        public static float BeatToFloat(int[] beat)
        {
            return ChartLoader.BeatToFloat(beat);
        }

        public static void NormalizeBeat(int[] beat)
        {
            ChartLoader.NormalizeBeat(beat);
            beat[0] = beat[1] / beat[2];
            beat[1] %= beat[2];
        }

        public static NotePosition GetPosition(Note note)
        {
            return GetPosition(note.lane, note.beat);
        }

        public static NotePosition GetPosition(int lane, int[] beat)
        {
            return new NotePosition(lane, beat[0] * beat[2] + beat[1], beat[2]);
        }

        public static bool IsFuwafuwa(Note note)
        {
            return note.lane == -1;
        }

        public static string ToString(int[] beat)
        {
            return $"{beat[0]}:{beat[1]}/{beat[2]}";
        }
    }
}
