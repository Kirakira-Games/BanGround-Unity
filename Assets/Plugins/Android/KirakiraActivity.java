package fun.banground.game;

import android.content.ContentResolver;
import android.content.pm.PackageManager;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayerActivity;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;

public class KirakiraActivity extends UnityPlayerActivity {
    private static final int REQUEST_EXTERNAL_STORAGE = 1;
    private static String[] PERMISSIONS_STORAGE = {
            "android.permission.READ_EXTERNAL_STORAGE",
            "android.permission.WRITE_EXTERNAL_STORAGE" };

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        while(checkSelfPermission("android.permission.WRITE_EXTERNAL_STORAGE") != PackageManager.PERMISSION_GRANTED)
        {
            requestPermissions(PERMISSIONS_STORAGE,REQUEST_EXTERNAL_STORAGE);
        }

        Intent intent = getIntent();
        String action = intent.getAction();

        assert action != null;
        if (action.compareTo("android.intent.action.VIEW") == 0) {
            String scheme = intent.getScheme();
            ContentResolver resolver = getContentResolver();

            assert scheme != null;
            if (scheme.compareTo("file") == 0) {
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
                if(!inbox.exists())
                    inbox.mkdir();

                InputStreamToFile(input, new File(inbox, name));
            }
        }
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
        } catch (Exception e) {
            Log.e("MainActivity", "InputStreamToFile exception: " + e.getMessage());
        }
    }


    public File getAndroidStorageFile() {
        return this.getFilesDir();
    }
}