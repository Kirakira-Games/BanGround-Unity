using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace BanGround.Audio
{
    class AudioFileAbstraction : TagLib.File.IFileAbstraction
    {
        const string BASE_NAME = "audio";

        private string ext = null;
        private byte[] audioBytes = null;

        public string Name => BASE_NAME + ext;

        public Stream ReadStream { get; private set; }
        public Stream WriteStream { get; private set; }

        public AudioFileAbstraction(byte[] bytes)
        {
            audioBytes = bytes;

            var stream = new MemoryStream(audioBytes);
            ReadStream = stream;
            WriteStream = stream;

            using (var br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                string header = new string(br.ReadChars(4));

                if (header == "OggS")
                    ext = ".ogg";

#if MORE_FORMATS
                if (header == "fLaC")
                    ext = ".flac";

                if (header == "ID3\x3")
                    ext = ".mp3";

                string mp4header = new string(br.ReadChars(8));

                if (mp4header == "ftypmp41" || mp4header == "ftypmp42" || mp4header == "ftypM4A ")
                    ext = ".aac";'
#endif
            }

            if (ext == null)
                throw new UnsupportedFormatException();
        }

        public void CloseStream(Stream stream) => stream.Dispose();
    }
}
