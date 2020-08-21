public interface IAudioTimelineSync
{
    float smoothAudioDiff { get; }
    float time { get; set; }
    int timeInMs { get; set; }

    float BGMTimeToRealtime(float t);
    int BGMTimeToRealtime(int t);
    void Pause();
    void Play();
    float RealTimeToBGMTime(float t);
    int RealTimeToBGMTime(int t);
}