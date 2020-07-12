#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine.Scripting;

namespace V2
{
    [Preserve]
    [ProtoContract()]
    public class NoteAnim : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1, IsPacked = true)]
        public int[] beat { get; set; }

        [ProtoMember(2)]
        public float x { get; set; }

        [ProtoMember(3)]
        public float y { get; set; }
    }

    [Preserve]
    [ProtoContract()]
    public class BPMPoint : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1, IsPacked = true)]
        public int[] beat { get; set; }

        [ProtoMember(2)]
        public float value { get; set; }
    }

    [Preserve]
    [ProtoContract()]
    public class TimingPoint : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1, IsPacked = true)]
        public int[] beat { get; set; }

        [ProtoMember(2)]
        public TransitionProperty<float> speed { get; set; }

        [ProtoMember(3, IsPacked = true)]
        public TransitionColor tap { get; set; }

        [ProtoMember(4, IsPacked = true)]
        public TransitionColor flick { get; set; }

        [ProtoMember(5, IsPacked = true)]
        public TransitionColor slideTick { get; set; }

        [ProtoMember(6, IsPacked = true)]
        public TransitionColor slide { get; set; }

        public static TimingPoint Lerp(TimingPoint a, TimingPoint b, float t)
        {
            TimingPoint ret = new TimingPoint
            {
                speed = new TransitionProperty<float>(TransitionLib.Lerp(a.speed, b.speed, t, a.speed.transition), a.speed.transition),
                tap = TransitionColor.Lerp(a.tap, b.tap, t),
                flick = TransitionColor.Lerp(a.flick, b.flick, t),
                slideTick = TransitionColor.Lerp(a.slideTick, b.slideTick, t),
                slide = TransitionColor.Lerp(a.slide, b.slide, t)
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
    }

    [Preserve]
    [ProtoContract()]
    public partial class Note : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        public NoteType type { get; set; }

        [ProtoMember(2, IsPacked = true)]
        public int[] beat { get; set; }

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
    }

    [Preserve]
    [ProtoContract()]
    public partial class Chart : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        public int version { get; set; }

        [ProtoMember(2)]
        public Difficulty Difficulty { get; set; }

        [ProtoMember(3)]
        public int level { get; set; }

        [ProtoMember(4)]
        public int offset { get; set; }

        [ProtoMember(5)]
        public List<TimingGroup> groups { get; set; } = new List<TimingGroup>();

        [ProtoMember(6)]
        public List<BPMPoint> bpm { get; set; } = new List<BPMPoint>();
    }
}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006