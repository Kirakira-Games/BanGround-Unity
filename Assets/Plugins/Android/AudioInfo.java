package fun.banground.game;

import android.app.Activity;
import android.content.Context;
import android.media.AudioManager;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class AudioInfo {
    private Context unityContext;
    private Activity unityActivity;
    private AudioManager audioManager;

    public void Init(Context _context){
        this.unityContext = _context.getApplicationContext();
        this.unityActivity = ((Activity)_context);
        audioManager = (AudioManager) unityActivity.getSystemService(Context.AUDIO_SERVICE);
    }

    public String GetSampleRate(){
        return audioManager.getProperty(AudioManager.PROPERTY_OUTPUT_SAMPLE_RATE);
    }

    public String GetBufferSize(){
        return audioManager.getProperty(AudioManager.PROPERTY_OUTPUT_FRAMES_PER_BUFFER);
    }

}
