#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using UnityEngine.Scripting;

namespace KiraPackOld
{
    [Preserve]
    [ProtoContract()]
    public partial class Chart : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        [System.ComponentModel.DefaultValue("")]
        public string author { get; set; } = "";

        [ProtoMember(2)]
        [System.ComponentModel.DefaultValue("")]
        public string authorUnicode { get; set; } = "";

        [ProtoMember(3)]
        [System.ComponentModel.DefaultValue("")]
        public string backgroundFile { get; set; } = "";

        [ProtoMember(4)]
        public Difficulty difficulty { get; set; }

        [ProtoMember(5)]
        [System.ComponentModel.DefaultValue("")]
        public string fileName { get; set; } = "";

        [ProtoMember(6)]
        public byte level { get; set; }

        [ProtoMember(7)]
        public int offset { get; set; }

        [ProtoMember(8)]
        public List<Note> notes = new List<Note>();

        [ProtoMember(9)]
        public int version { get; set; }
    }

    [Preserve]
    [ProtoContract()]
    public partial class Header : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        [System.ComponentModel.DefaultValue("")]
        public string Title { get; set; } = "";

        [ProtoMember(2)]
        [System.ComponentModel.DefaultValue("")]
        public string Artist { get; set; } = "";

        [ProtoMember(3)]
        [System.ComponentModel.DefaultValue("")]
        public string TitleUnicode { get; set; } = "";

        [ProtoMember(4)]
        [System.ComponentModel.DefaultValue("")]
        public string ArtistUnicode { get; set; } = "";

        [ProtoMember(5, IsPacked = true)]
        public float[] Preview { get; set; }

        [ProtoMember(6)]
        [System.ComponentModel.DefaultValue("")]
        public string DirName { get; set; } = "";

        [ProtoMember(7)]
        public System.Collections.Generic.List<Chart> charts = new System.Collections.Generic.List<Chart>();

    }

    [Preserve]
    [ProtoContract()]
    public partial class SongList : IExtensible
    {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        [System.ComponentModel.DefaultValue("")]
        public string GenerateDate { get; set; } = "";

        [ProtoMember(2)]
        public List<Header> songs = new List<Header>();

        public SongList() { }

        public SongList(string date, List<Header> list)
        {
            songs = list;
            GenerateDate = date;
        }

    }

}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006