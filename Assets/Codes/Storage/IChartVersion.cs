using Cysharp.Threading.Tasks;
using Difficulty = V2.Difficulty;

public interface IChartVersion
{
    bool CanConvert(int version);
    bool CanRead(int version);
    V2.Chart ConvertFromV1(cHeader header, Difficulty difficulty);
    UniTask<V2.Chart> Process(cHeader header, Difficulty difficulty);
}