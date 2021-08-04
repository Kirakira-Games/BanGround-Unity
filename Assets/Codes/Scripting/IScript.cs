namespace BanGround.Scripting
{
    public interface IScript
    {
        bool HasOnUpdate { get; }
        bool HasOnJudge { get; }
        bool HasOnBeat { get; }

        void Init(int sid, V2.Difficulty difficulty);
        void OnUpdate(int audioTime);
        void OnJudge(NoteBase notebase, JudgeResult result);
        void OnBeat(float beat);
    }
}
