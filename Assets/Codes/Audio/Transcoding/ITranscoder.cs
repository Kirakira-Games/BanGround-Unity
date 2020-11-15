using Cysharp.Threading.Tasks;
using System;

namespace BanGround.Audio
{
    interface ITranscoder : IDisposable
    {
        byte[] Source { get; set; }
        int Bitrate { get; set; }

        byte[] Do();
        UniTask<byte[]> DoAsync();
    }
}
