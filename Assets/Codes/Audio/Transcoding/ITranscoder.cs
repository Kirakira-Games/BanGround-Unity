using Cysharp.Threading.Tasks;
using System;

namespace BanGround.Audio
{
    interface ITranscoder : IDisposable
    {
        byte[] Source { get; set; }
        int Bitrate { get; set; }
        float Progress { get; }

        byte[] Do();
        UniTask<byte[]> DoAsync();
    }
}
