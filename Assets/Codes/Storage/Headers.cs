using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using Zenject.Internal;

namespace V2 {
    [Preserve]
    [ProtoContract()]
    public partial class SongList : IExtensible {
        private IExtension __pbn__extensionData;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoMember(1)]
        public List<cHeader> cHeaders { get; set; } = new List<cHeader>();

        [ProtoMember(2)]
        public List<mHeader> mHeaders { get; set; } = new List<mHeader>();
    }

    [Preserve]
    public partial class cHeader : IExtensible {
        public List<int> difficultyLevel;
        public void LoadDifficultyLevels(IDataLoader dataLoader) {
            if (difficultyLevel != null)
                return;
            difficultyLevel = new List<int>();
            for (var diff = V2.Difficulty.Easy; diff <= V2.Difficulty.Special; diff++) {
                var chart = dataLoader.GetChartPath(sid, diff);
                difficultyLevel.Add(dataLoader.GetChartLevel(chart));
            }
        }

        public void Sanitize(mHeader musicHeader) {
            if (preview == null || preview.Length == 0)
                preview = musicHeader.preview.ToArray();
            else if (preview.Length == 1)
                preview = new float[] { preview[0], preview[0] };
            else
                preview = preview.Take(2).ToArray();
        }
    }

    [Preserve]
    public partial class mHeader : IExtensible {
        private float[] SanitizeArray(float[] arr, float[] defaultValue) {
            if (arr == null || arr.Length == 0)
                return defaultValue;
            else if (arr.Length == 1)
                return new float[] { arr[0], arr[0] };
            else
                return arr.Take(2).ToArray();
        }

        public void Sanitize() {
            bpm = SanitizeArray(bpm, new float[] { 120, 120 });
            preview = SanitizeArray(preview, new float[] { 0, length });
        }
    }

}
