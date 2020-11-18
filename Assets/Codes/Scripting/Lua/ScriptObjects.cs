using AudioProvider;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

namespace BanGround.Scripting.Lunar
{
    [LuaCallCSharp]
    public class ScriptCamera
    {
        GameObject mainCamera;
        GameObject noteCamera;

        Vector3 initalPostion;
        Quaternion initalRotation;

        public ScriptCamera()
        {
            mainCamera = GameObject.Find("GameMainCamera");
            noteCamera = GameObject.Find("NoteCam");

            initalPostion = mainCamera.transform.position;
            initalRotation = mainCamera.transform.rotation;
        }

        public void SetPosition(float x, float y, float z)
        {
            var newPos = initalPostion + new Vector3(x, y, z);
            mainCamera.transform.position = newPos;
            noteCamera.transform.position = newPos;
        }

        public void SetRotation(float pitch, float yaw, float roll)
        {
            var newAngle = initalRotation;
            newAngle.eulerAngles += new Vector3(pitch, yaw, roll);

            mainCamera.transform.rotation = newAngle;
            noteCamera.transform.rotation = newAngle;
        }
    }

    [LuaCallCSharp]
    public class ScriptSoundEffect : IDisposable
    {
        ISoundEffect se;

        public ScriptSoundEffect(UniTask<ISoundEffect> seTask)
        {
            Await(seTask);
        }

        async void Await(UniTask<ISoundEffect> seTask)
        {
            se = await seTask;
        }

        public void Dispose()
        {
            se?.Dispose();
        }

        public void PlayOneShot()
        {
            se?.PlayOneShot();
        }
    }

    [LuaCallCSharp]
    public class ScriptSprite : IDisposable
    {
        Func<int, Texture2D> texLookup;

        Sprite spr;
        
        GameObject obj;
        SpriteRenderer spriteRenderer;

        public ScriptSprite(int texId, Func<int, Texture2D> texLookup)
        {
            this.texLookup = texLookup;

            obj = new GameObject("ScriptSprite");
            obj.transform.parent = GameObject.Find("ScriptObjects").transform;

            spriteRenderer = obj.AddComponent<SpriteRenderer>();

            OverrideTexture(texId);
        }
        
        public void SetColor(float r, float g, float b, float a)
        {
            spriteRenderer.color = new Color(r, g, b, a);
        }

        public void SetPosition(float x, float y, float z)
        {
            obj.transform.position = new Vector3(x, y, z);
        }

        public void SetRotation(float pitch, float yaw, float roll)
        {
            obj.transform.rotation = Quaternion.Euler(pitch, yaw, roll);
        }

        public void OverrideTexture(int texId)
        {
            if(spr != null)
                UnityEngine.Object.Destroy(spr);

            var tex = texLookup(texId);
            spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            spriteRenderer.sprite = spr;
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(obj);
            UnityEngine.Object.Destroy(spr);
        }
    }
}
