using Cysharp.Threading.Tasks;

namespace BanGround.Community
{
    public interface IStoreController
    {
        UniTaskVoid LoadCharts(SongItem song, int offset);
        UniTaskVoid Search(string text, int offset = 0);
    }
}