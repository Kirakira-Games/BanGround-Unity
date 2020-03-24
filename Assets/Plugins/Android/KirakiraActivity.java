package fun.banground.game;

import android.content.ContentResolver;
import android.content.pm.PackageManager;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;

import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;

/*
class CallbackThread extends Thread {
    public void run() {
        try {
            while (KirakiraActivity.fileImportCallback == null) {
                Thread.sleep(100);
            }
            KirakiraActivity.fileImportCallback.onFileImport();
        } catch (InterruptedException e) {}
    }
}
*/

public class KirakiraActivity extends UnityPlayerActivity {
    private static final int REQUEST_EXTERNAL_STORAGE = 1;
    private static String[] PERMISSIONS_STORAGE = {
            "android.permission.READ_EXTERNAL_STORAGE",
            "android.permission.WRITE_EXTERNAL_STORAGE" };
    /*
    public static FileImportCallback fileImportCallback;

    public static void registerFileImportCallback(FileImportCallback callback) {
        fileImportCallback = callback;
    }
    */
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        ImportFile(getIntent());
    }

    private void InputStreamToFile(InputStream in, File file) {
        try {
            OutputStream out = new FileOutputStream(file);

            byte[] buffer = new byte[1024];
            int size;
            while ((size = in.read(buffer)) != -1) {
                out.write(buffer, 0, size);
            }
            out.close();

            // new CallbackThread().start();
        } catch (Exception e) {
            Log.e("MainActivity", "InputStreamToFile exception: " + e.getMessage());
        }
    }

    public void ImportFile(Intent intent){
        while(checkSelfPermission("android.permission.WRITE_EXTERNAL_STORAGE") != PackageManager.PERMISSION_GRANTED)
        {
            requestPermissions(PERMISSIONS_STORAGE,REQUEST_EXTERNAL_STORAGE);
        }

        String action = intent.getAction();

        assert action != null;
        if (action.compareTo("android.intent.action.VIEW") == 0) {
            String scheme = intent.getScheme();
            ContentResolver resolver = getContentResolver();

            assert scheme != null;
            if (scheme.compareTo("file") == 0 || scheme.compareTo("content") == 0) {
                InputStream input;
                Uri uri = intent.getData();
                String name = uri.getLastPathSegment();

                Log.v("tag", "File intent detected: " + action + " : " + intent.getDataString() + " : " + intent.getType() + " : " + name);

                try {
                    input = resolver.openInputStream(uri);
                } catch (FileNotFoundException e) {
                    e.printStackTrace();
                    return;
                }

                File inbox = new File(getAndroidStorageFile(), "/Inbox/");
                Log.v("Unity",inbox.getAbsolutePath());
                if(!inbox.exists())
                    inbox.mkdir();

                InputStreamToFile(input, new File(inbox, name));
            }
        }
    }

    public File getAndroidStorageFile() {
        return this.getExternalFilesDir(null);
    }
    
    @Override public void onWindowFocusChanged(boolean hasFocus)
    {
        super.onWindowFocusChanged(hasFocus);

        if (hasFocus) {
            hideSystemUI();
        }
    }

    @Override public void onNewIntent(Intent intent)
    {
        ImportFile(intent);
        super.onNewIntent(intent);
    }

    private void hideSystemUI() {
        // 沉浸 SYSTEM_UI_FLAG_IMMERSIVE.
        // 粘性沉浸 SYSTEM_UI_FLAG_IMMERSIVE_STICKY
        View decorView = getWindow().getDecorView();
        decorView.setSystemUiVisibility(
                View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
                        // 全屏 隐藏导航栏 隐藏状态栏
                        | View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                        | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                        | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                        | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                        | View.SYSTEM_UI_FLAG_FULLSCREEN);
    }
}