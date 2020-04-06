using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLua;
using System.IO;
using UnityEngine;
using AudioProvider;

[LuaCallCSharp]
class ChartCamera
{
    GameObject mainCamera;
    GameObject noteCamera;

    Vector3 initalPostion;
    Quaternion initalRotation;

    public ChartCamera()
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
class ScriptSoundEffect : IDisposable
{
    ISoundEffect se;

    public ScriptSoundEffect(string file)
    {
        var path = ChartScript.GetChartResource(file);

        if(KiraFilesystem.Instance.Exists(path))
        {
            se = AudioManager.Instance.PrecacheSE(KiraFilesystem.Instance.Read(path));
        }
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

[CSharpCallLua]
class ChartScript : IDisposable
{
    LuaEnv luaEnv = null;
    LuaFunction onUpdate = null;

    private static int sid;

    public static string GetChartResource(string filename)
    {
        return DataLoader.ChartDir + sid + "/" + filename;
    }

    public static string GetChartScriptPath(Difficulty difficulty)
    {
        return GetChartResource(difficulty.ToString("G").ToLower() + ".lua");
    }

    public ChartScript(int chartSetId, Difficulty difficulty)
    {
        sid = chartSetId;

        var path = GetChartScriptPath(difficulty);

        if (KiraFilesystem.Instance.Exists(path))
        {
            var script = KiraFilesystem.Instance.ReadString(path);

            luaEnv = new LuaEnv();
            luaEnv.DoString(script);

            onUpdate = luaEnv.Global.Get<LuaFunction>("OnUpdate");
        }
    }

    public void OnUpdate(int audioTime)
    {
        onUpdate?.Call(audioTime);
    }

    public void Dispose()
    {
        onUpdate = null;
        luaEnv.Dispose();
    }
}