namespace BGEditor
{
    public interface IAudioProgressController : IInitable
    {
        uint audioLength { get; }
        bool canSeek { get; }

        void ChangePlaybackRate(float value);
        void IncreasePlaybackRate(int delta);
        void IncreasePosition(float delta);
        void Pause();
        void Play();
        void Refresh();
        float ScrollPosToTime(float pos);
        void Switch();
        float TimeToScrollPos(float time);
        void UpdateDisplay(bool isUser);
    }
}