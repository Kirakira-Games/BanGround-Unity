using UnityEngine;
using System.Collections;

public class BassAudioSource : MonoBehaviour
{
    private AudioManager audioMgr;
    private BassMemStream stream;
    private float _volume;

    public TextAsset clip;
    public TextAsset Clip
    {
        get
        {
            return clip;
        }
        set
        {
            if (stream != null && !stream.IsDisposed)
                stream.Dispose();
                 
            clip = value;
            if (loop)
                stream = audioMgr.StreamSound(value.bytes, Un4seen.Bass.BASSFlag.BASS_MUSIC_LOOP);
            else
                stream = audioMgr.StreamSound(value.bytes);
        }
    }

    public bool playOnAwake;
    public bool loop;
    [Range(0, 1)]
    public float volume = 0.75f;
    public float position => stream.Position;

    // Use this for initialization
    void Start()
    {
        audioMgr = AudioManager.Instanse;

        Clip = clip;

        stream.Volume = volume;
        _volume = volume;

        if(playOnAwake)
            stream.Play();
    }

    public void Play(bool restart = false)
    {
        stream.Play(restart);
    }

    public void Pause()
    {
        stream.Pause();
    }

    public void Stop()
    {
        stream.Stop();
    }

    void Update()
    {
        if(volume != _volume)
        {
            _volume = volume;
            stream.Volume = volume;
        }
    }

    void OnDestroy()
    {
        stream?.Dispose();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            Pause();
        else
            Play();
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}
