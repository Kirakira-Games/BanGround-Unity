using UnityEngine;

public interface IInGameBackground
{
    void pauseVideo();
    void playVideo();
    void seekVideo(double sec);
    void SetBackground(string path, int type);
    void SetBackground(Texture2D tex);
    void stopVideo();
}
