public interface IAudioTimelineSync
{
    float SmoothAudioDiff { get; }
    float Time { get; set; }
    int TimeInMs { get; set; }
    float AudioSeekPos { get; set; }

    float BGMTimeToRealtime(float t);
    int BGMTimeToRealtime(int t);
    void Pause();
    void Play();
    float RealTimeToBGMTime(float t);
    int RealTimeToBGMTime(int t);
}