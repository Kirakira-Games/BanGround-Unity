using AudioProvider;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Zenject;

namespace BanGround.Audio
{
    class BassTranscoder : ITranscoder
    {
        [Inject]
        IAudioProvider provider;

        public byte[] Source { get; set; } = null;
        public int Bitrate { get; set; } = 96;

        private static readonly string[] PluginNames =
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        {"bassenc", "bassenc_opus"};
#elif UNITY_ANDROID
        {"libbassenc", "libbassenc_opus"};
#elif UNITY_IOS
        {"BASSENC", "BASSENC_OPUS"};
#endif

        private List<int> LoadedPlugins = new List<int>();

        public BassTranscoder()
        {
            if(!(provider is BassAudioProvider))
                Bass.BASS_Init(0, 48000, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            foreach(var plugin in PluginNames)
            {
                int handle = Bass.BASS_PluginLoad(plugin);

                if (handle == 0)
                    throw new Exception($"Failed to load plugin {plugin}!");

                LoadedPlugins.Add(handle);
            }
        }

        public byte[] Do()
        {
            if(Source == null)
            {
                throw new ArgumentNullException("Source not setted!");
            }

            return null;
        }

        public UniTask<byte[]> DoAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var plugin in LoadedPlugins)
                Bass.BASS_PluginFree(plugin);

            if (!(provider is BassAudioProvider))
                Bass.BASS_Free();
        }
    }
}
