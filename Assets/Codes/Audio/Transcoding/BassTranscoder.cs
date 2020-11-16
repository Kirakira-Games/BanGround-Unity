using AudioProvider;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Enc;
using Un4seen.Bass.AddOn.EncOpus;
using Zenject;

using Debug = UnityEngine.Debug;

namespace BanGround.Audio
{
    class BassTranscoder : ITranscoder
    {
        [Inject]
        IAudioProvider provider;

        public byte[] Source { get; set; } = null;
        public int Bitrate { get; set; } = 96;
        public float Progress { get; private set; } = .0f;

        public BassTranscoder()
        {
            if(!(provider is BassAudioProvider))
                Bass.BASS_Init(0, 48000, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }

        List<byte> _result = new List<byte>();

        public byte[] Do()
        {
            if(Source == null)
            {
                throw new ArgumentNullException("Source not setted!");
            }

            var pinnedObject = GCHandle.Alloc(Source, GCHandleType.Pinned);
            var pinnedObjectPtr = pinnedObject.AddrOfPinnedObject();

            var id = Bass.BASS_StreamCreateFile(pinnedObjectPtr, 0, Source.Length, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
            var encoder = BassEnc_Opus.BASS_Encode_OPUS_Start(id, $"--bitrate {Bitrate}", BASSEncode.BASS_ENCODE_DEFAULT, EncodeCallback, IntPtr.Zero);

            var size = Bass.BASS_ChannelGetLength(id);
            float fullSize = size;

            var buffer = new byte[65536];

            while (true)
            {
                var transferLength = size;
                if (transferLength > buffer.Length)
                    transferLength = buffer.Length;

                var transferred = Bass.BASS_ChannelGetData(id, buffer, (int)transferLength);

                Progress = 1.0f - (size / fullSize);

                Debug.Log($"Encode progress {Progress}");

                if (transferred < 1)
                    break;

                size -= transferred;
            }

            BassEnc.BASS_Encode_Stop(encoder);
            Bass.BASS_StreamFree(id);

            return _result.ToArray();
        }

        public unsafe void EncodeCallback(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            byte* pBuffer = (byte*)buffer.ToPointer();

            while (length-- > 0)
                _result.Add(*(pBuffer++));
        }

        public UniTask<byte[]> DoAsync()
        {
            return UniTask.Run(Do);
        }

        public void Dispose()
        {
            if (!(provider is BassAudioProvider))
                Bass.BASS_Free();
        }
    }
}
