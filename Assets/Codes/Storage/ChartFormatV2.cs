#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using V1Chart = Chart;
using V1Note = Note;
using ChartUtility = BGEditor.ChartUtility;

namespace V2
{
    public interface IWithTiming
    {
        int[] beat { get; set; }
        float time { get; set; }
        float beatf { get; set; }
    }

    [Preserve]
    [ProtoContract()]
    public class NoteAnim : IExtensible, IWithTiming
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1, IsPacked = true)]
        public int[] beat { get; set; }
        [JsonIgnore]
        public float time { get; set; }
        [JsonIgnore]
        public float beatf { get; set; }

        [ProtoMember(2)]
        public TransitionVector pos { get; set; }

        public static NoteAnim Lerp(NoteAnim a, NoteAnim b, float t)
        {
            t = Mathf.Clamp01(t);
            return LerpUnclamped(a, b, t);
        }

        public static NoteAnim LerpUnclamped(NoteAnim a, NoteAnim b, float t)
        {
            return new NoteAnim
            {
                beatf = Mathf.LerpUnclamped(a.beatf, b.beatf, t),
                time = Mathf.LerpUnclamped(a.time, b.time, t),
                pos = TransitionVector.LerpUnclamped(a.pos, b.pos, t)
            };
        }

        public static string ToString(IWithTiming t)
        {
            if (t.beat != null)
                return $"[{t.beat[0]}:{t.beat[1]}/{t.beat[2]}] ";
            else
                return $"[{t.time}] ";
        }

        public override string ToString()
        {
            return $"{ToString(this)} pos={pos}";
        }
    }

    [Preserve]
    [ProtoContract()]
    public class ValuePoint : IExtensible, IWithTiming
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1, IsPacked = true)]
        public int[] beat { get; set; }
        [JsonIgnore]
        public float time { get; set; }
        [JsonIgnore]
        public float beatf { get; set; }

        [ProtoMember(2)]
        public float value { get; set; }

        public override string ToString()
        {
            return $"{NoteAnim.ToString(this)}: {value}";
        }
    }

    [Preserve]
    [ProtoContract()]
    public class TimingPoint : IExtensible, IWithTiming
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1, IsPacked = true)]
        public int[] beat { get; set; }
        [JsonIgnore]
        public float time { get; set; }
        [JsonIgnore]
        public float beatf { get; set; }

        [ProtoMember(2)]
        public TransitionProperty<float> speed { get; set; }

        [ProtoMember(3, IsPacked = true)]
        public TransitionColor tap { get; set; }

        [ProtoMember(4, IsPacked = true)]
        public TransitionColor tapGrey { get; set; }

        [ProtoMember(5, IsPacked = true)]
        public TransitionColor flick { get; set; }

        [ProtoMember(6, IsPacked = true)]
        public TransitionColor slideTick { get; set; }

        [ProtoMember(7, IsPacked = true)]
        public TransitionColor slide { get; set; }

        public static TimingPoint Default()
        {
            return new TimingPoint
            {
                beat = new int[] { -100, 0, 1 },
                speed = new TransitionProperty<float>(1f),
                tap = new TransitionColor(113, 237, 255),
                tapGrey = new TransitionColor(180, 180, 180),
                flick = new TransitionColor(255, 59, 114),
                slideTick = new TransitionColor(84, 230, 44),
                slide = new TransitionColor(84, 230, 44)
            };
        }

        public static TimingPoint Lerp(TimingPoint a, TimingPoint b, float t)
        {
            t = Mathf.Clamp01(t);
            return LerpUnclamped(a, b, t);
        }

        public static TimingPoint LerpUnclamped(TimingPoint a, TimingPoint b, float t)
        {
            TimingPoint ret = new TimingPoint
            {
                time = Mathf.Lerp(a.time, b.time, t),
                speed = new TransitionProperty<float>(TransitionLib.LerpUnclamped(a.speed, b.speed, t, a.speed.transition), a.speed.transition),
                tap = TransitionColor.LerpUnclamped(a.tap, b.tap, t),
                tapGrey = TransitionColor.LerpUnclamped(a.tapGrey, b.tapGrey, t),
                flick = TransitionColor.LerpUnclamped(a.flick, b.flick, t),
                slideTick = TransitionColor.LerpUnclamped(a.slideTick, b.slideTick, t),
                slide = TransitionColor.LerpUnclamped(a.slide, b.slide, t)
            };
            return ret;
        }

        public TimingPoint Copy()
        {
            return new TimingPoint
            {
                beat = beat.ToArray(),
                time = time,
                speed = speed.Copy(),
                tap = tap.Copy(),
                tapGrey = tapGrey.Copy(),
                flick = flick.Copy(),
                slideTick = slideTick.Copy(),
                slide = slide.Copy()
            };
        }

        public override string ToString()
        {
            return $"{NoteAnim.ToString(this)} speed={speed} tap={tap} tapGrey={tapGrey} flick={flick} slideTick={slideTick} slide={slide}";
        }

        public string ToEditorString()
        {
            string sTap = TransitionColor.ColoredString("〇", tap);
            string sTapGrey = TransitionColor.ColoredString("〇", tapGrey);
            string sFlick = TransitionColor.ColoredString("〇", flick);
            string sSlideTick = TransitionColor.ColoredString("〇", slideTick);
            string sSlide = TransitionColor.ColoredString("〇", slide);
            return $"Speed:{speed.value} {sTap}{sTapGrey}{sFlick}{sSlideTick}{sSlide}";
        }
    }

    [Preserve]
    [ProtoContract()]
    public class TimingGroup : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        public List<Note> notes { get; set; } = new List<Note>();

        [ProtoMember(2)]
        public List<TimingPoint> points { get; set; } = new List<TimingPoint>();

        [ProtoMember(3)]
        public uint flags { get; set; }

        public static TimingGroup From(List<V1Note> notes)
        {
            var ret = Default();
            ret.notes = notes.Where(note => note.type != NoteType.BPM).Select(note => Note.From(note)).ToList();
            return ret;
        }

        public static TimingGroup Default()
        {
            var ret = new TimingGroup();
            ret.points.Add(TimingPoint.Default());
            return ret;
        }
    }

    [Preserve]
    [ProtoContract()]
    public partial class Note : IExtensible, IWithTiming
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        public NoteType type { get; set; }

        [ProtoMember(2, IsPacked = true)]
        public int[] beat { get; set; }
        [JsonIgnore]
        public float time { get; set; }
        [JsonIgnore]
        public float beatf { get; set; }

        [ProtoMember(3)]
        public int lane { get; set; }

        [ProtoMember(4)]
        public int tickStack { get; set; }

        [ProtoMember(5)]
        public List<NoteAnim> anims { get; set; } = new List<NoteAnim>();

        [ProtoMember(6)]
        public float x { get; set; }

        [ProtoMember(7)]
        public float y { get; set; }
        public float yOrNaN => lane >= 0 ? float.NaN : y;

        [ProtoMember(8)]
        public uint flags { get; set; }

        [JsonIgnore]
        public int group { get; set; }

        public static Note From(V1Note note)
        {
            return new Note
            {
                type = note.type,
                beat = note.beat.ToArray(),
                lane = note.lane,
                tickStack = note.tickStack < 0 ? 0 : note.tickStack + 1,
                x = note.x,
                y = note.y
            };
        }

        public override string ToString()
        {
            return $"{NoteAnim.ToString(this)} lane={lane}, x={x}, y={y}, ts={tickStack}, anims=[\n  {string.Join("\n  ", anims.Select(anim => anim.ToString()))}\n]";;
        }
    }

    [Preserve]
    [ProtoContract()]
    public partial class Chart : IExtensible
    {
        public const int VERSION = 2;

        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        public Difficulty difficulty { get; set; }

        [ProtoMember(2)]
        public int level { get; set; }

        [ProtoMember(3)]
        public int offset { get; set; }

        [ProtoMember(4)]
        public List<TimingGroup> groups { get; set; } = new List<TimingGroup>();

        [ProtoMember(5)]
        public List<ValuePoint> bpm { get; set; } = new List<ValuePoint>();

        [ProtoMember(6)]
        public int version { get; set; } = VERSION;

        public float BeatToTime(float beat)
        {
            float ret = offset / 1000f;

            for (int i = 0; i < bpm.Count; i++)
            {
                var cur = bpm[i];
                float start = ChartUtility.BeatToFloat(cur.beat);
                float end = i == bpm.Count - 1 ? 1e9f : ChartUtility.BeatToFloat(bpm[i + 1].beat);
                float timeperbeat = 60 / cur.value;
                if (beat <= end)
                {
                    ret += (beat - start) * timeperbeat;
                    return ret;
                }
                ret += (end - start) * timeperbeat;
            }

            throw new ArgumentOutOfRangeException(beat + " cannot be converted to time.");
        }

        public float BeatToTime(int[] beat) { return BeatToTime(ChartUtility.BeatToFloat(beat)); }
        public int BeatToTimeMS(float beat) { return Mathf.RoundToInt(BeatToTime(beat) * 1000); }
        public int BeatToTimeMS(int[] beat) { return BeatToTimeMS(ChartUtility.BeatToFloat(beat)); }

        public float TimeToBeat(float audioTimef)
        {
            audioTimef -= offset / 1000f;
            if (audioTimef - NoteUtility.EPS <= 0)
                return 0;

            for (int i = 0; i < bpm.Count; i++)
            {
                var cur = bpm[i];
                float timeperbeat = 60 / cur.value;
                float start = ChartUtility.BeatToFloat(cur.beat);
                float end = i == bpm.Count - 1 ? 1e9f : ChartUtility.BeatToFloat(bpm[i + 1].beat);
                float duration = (end - start) * timeperbeat;
                if (audioTimef <= duration)
                {
                    return start + audioTimef / timeperbeat;
                }
                audioTimef -= duration;
            }

            throw new ArgumentOutOfRangeException(audioTimef + " cannot be converted to beat.");
        }

        public static Chart From(V1Chart old)
        {
            return new Chart
            {
                version = VERSION,
                difficulty = old.Difficulty,
                level = old.level,
                offset = old.offset,
                groups = new List<TimingGroup> { TimingGroup.From(old.notes) },
                bpm = old.notes.Where(note => note.type == NoteType.BPM).Select(note => new ValuePoint
                {
                    beat = note.beat.ToArray(),
                    value = note.value
                }).ToList()
            };
        }
    }
}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006