#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Scripting;
using V1Chart = Chart;
using V1Note = Note;

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
                beat = new int[] { 0, 0, 1 },
                speed = new TransitionProperty<float>(1f),
                tap = new TransitionColor(0, 0, 255),
                tapGrey = new TransitionColor(128, 128, 128),
                flick = new TransitionColor(255, 0, 0),
                slideTick = new TransitionColor(0, 255, 0),
                slide = new TransitionColor(0, 255, 0)
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
                speed = new TransitionProperty<float>(TransitionLib.LerpUnclamped(a.speed, b.speed, t, a.speed.transition), a.speed.transition),
                tap = TransitionColor.LerpUnclamped(a.tap, b.tap, t),
                tapGrey = TransitionColor.LerpUnclamped(a.tapGrey, b.tapGrey, t),
                flick = TransitionColor.LerpUnclamped(a.flick, b.flick, t),
                slideTick = TransitionColor.LerpUnclamped(a.slideTick, b.slideTick, t),
                slide = TransitionColor.LerpUnclamped(a.slide, b.slide, t)
            };
            return ret;
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
            var ret = new TimingGroup
            {
                notes = notes.Select(note => Note.From(note)).ToList()
            };
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

        [ProtoMember(8)]
        public uint flags { get; set; }

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
        public int version { get; set; }

        [ProtoMember(2)]
        public Difficulty difficulty { get; set; }

        [ProtoMember(3)]
        public int level { get; set; }

        [ProtoMember(4)]
        public int offset { get; set; }

        [ProtoMember(5)]
        public List<TimingGroup> groups { get; set; } = new List<TimingGroup>();

        [ProtoMember(6)]
        public List<ValuePoint> bpm { get; set; } = new List<ValuePoint>();

        public static Chart From(V1Chart old)
        {
            return new Chart
            {
                version = VERSION,
                difficulty = old.Difficulty,
                level = old.level,
                offset = old.offset,
            };
        }
    }
}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006