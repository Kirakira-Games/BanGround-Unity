using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XLua;
using Zenject;

namespace BanGround.Scripting.Lunar
{
    [LuaCallCSharp]
    public class LunarBanGroundAPI : IDisposable
    {
        internal IFileSystem fs;
        internal IDataLoader dl;
        internal IAudioManager am;
        internal LunarScript ls;
        internal int sid;

        IInGameBackground inGameBackground = null;

        List<Texture2D> loadedTextures = new List<Texture2D>();
        List<IDisposable> disposableObjects = new List<IDisposable>();

        public ScriptCamera GetCamera() => new ScriptCamera();

        public int LoadTexture(string tex)
        {
            var path = dl.GetChartResource(sid, tex);

            if(fs.FileExists(path))
            {
                var t = fs.GetFile(path).ReadAsTexture();
                loadedTextures.Add(t);

                return loadedTextures.Count - 1;
            }

            return -1;
        }

        public void SetBackground(int texId)
        {
            if(inGameBackground == null)
                inGameBackground = GameObject.Find("InGameBackground").GetComponent<InGameBackground>();

            inGameBackground.SetBackground(loadedTextures[texId]);
        }

        public void SetLaneBackground(int texId)
        {
            var mesh = GameObject.Find("LaneBackground").GetComponent<MeshRenderer>();
            var material = UnityEngine.Object.Instantiate(mesh.sharedMaterial);
            material.SetTexture("_BaseMap", loadedTextures[texId]);

            mesh.sharedMaterial = material;
        }
        public void SetJudgeLineColor(float r, float g, float b)
        {
            var mesh = GameObject.Find("JudgeLine").GetComponent<MeshRenderer>();
            var material = UnityEngine.Object.Instantiate(mesh.sharedMaterial);
            material.SetColor("_BaseColor", new Color(r, g, b));

            mesh.sharedMaterial = material;
        }

        public ScriptSoundEffect PrecacheSound(string snd)
        {
            var path = dl.GetChartResource(sid, snd);

            if (fs.FileExists(path))
            {
                var t = fs.GetFile(path).ReadToEnd();
                var task = am.PrecacheSE(t);

                var se = new ScriptSoundEffect(task);
                disposableObjects.Add(se);

                return se;
            }

            return null;
        }

        public ScriptSprite CreateSprite(int textureId)
        {
            if (textureId == -1 && loadedTextures.Count < textureId)
                return null;

            var spr = new ScriptSprite(textureId, id => loadedTextures[id]);
            disposableObjects.Add(spr);

            return spr;
        }

        public float GetHealth() => LifeController.instance.lifePoint;

        float startTime = 0;
        float startBeat = 0;

        public void UntilTime(float time, LuaFunction callback)
        {
            ls.AddKeyframeByTime(startTime, time, callback);
            startTime = time;
        }

        public void UntilBeat(float beat, LuaFunction callback)
        {
            ls.AddKeyframeByBeat(startBeat, beat, callback);
            startBeat = beat;
        }

        public void Msg(object obj)
        {
            if (obj == null)
                Debug.Log("Null");
            else
                Debug.Log(obj.ToString());
        }

        public void Dispose()
        {
            foreach (var obj in disposableObjects)
                obj.Dispose();

            foreach (var tex in loadedTextures)
                UnityEngine.Object.Destroy(tex);
        }
    }

    [CSharpCallLua]
    public class LunarScript : MonoBehaviour, IScript
    {
        [Inject]
        IDataLoader dataLoader;
        [Inject]
        IFileSystem fs;
        [Inject]
        IAudioManager am;
        [Inject]
        private IChartLoader chartLoader;

        LuaEnv luaEnv = null;
        LuaFunction onUpdate = null;
        LuaFunction onBeat = null;
        LuaFunction onJudge = null;

        int curKeyframe = 0;
        List<(float, float, LuaFunction)> keyframes = new List<(float, float, LuaFunction)>();

        int sid = 0;

        public bool HasOnUpdate => onUpdate != null;
        public bool HasOnJudge => onJudge != null;
        public bool HasOnBeat => onBeat != null;

        public void Init(int sid, Difficulty difficulty)
        {
            var scriptPath = dataLoader.GetChartScriptPath(sid, difficulty);
            
            if(fs.FileExists(scriptPath))
            {
                this.sid = sid;
                var scriptFile = fs.GetFile(scriptPath);

                luaEnv = new LuaEnv();

                luaEnv.AddLoader((ref string lib) =>
                {
                    var modulepath = dataLoader.GetChartResource(sid, lib + ".lua");

                    if (fs.FileExists(modulepath))
                    {
                        return fs.GetFile(modulepath).ReadToEnd();
                    }

                    return null;
                });

                var api = new LunarBanGroundAPI
                {
                    fs = fs,
                    dl = dataLoader,
                    am = am,
                    sid = sid,
                    ls = this
                };

                luaEnv.Global.Set("BanGround", api);

                luaEnv.DoString(scriptFile.ReadAsString());

                onUpdate = luaEnv.Global.Get<LuaFunction>("OnUpdate");
                onJudge = luaEnv.Global.Get<LuaFunction>("OnJudge");
                onBeat = luaEnv.Global.Get<LuaFunction>("OnBeat");
            }
        }

        public void AddKeyframeByTime(float startTime, float time, LuaFunction callback)
        {
            keyframes.Add((startTime, time - startTime, callback));
            keyframes.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        }

        public void AddKeyframeByBeat(float startBeat, float beat, LuaFunction callback)
        {
            float sb = chartLoader.chart.BeatToTime(startBeat),
                b = chartLoader.chart.BeatToTime(beat);

            AddKeyframeByTime(sb, b, callback);
        }

        public void OnBeat(float beat)
        {
            onBeat?.Call(beat);
        }

        public void OnJudge(NoteBase notebase, JudgeResult result)
        {
            onJudge?.Call(new JudgeResultObj
            {
                Lane = notebase.lane,
                Type = (int)notebase.type,
                Time = notebase.time,
                Beat = chartLoader.chart.TimeToBeat(notebase.time / 1000.0f),
                JudgeResult = (int)result,
                JudgeTime = notebase.judgeTime,
                JudgeOffset = notebase.time - notebase.judgeTime
            });
        }

        public void OnUpdate(int audioTime)
        {
            onUpdate?.Call(audioTime);

            if (keyframes.Count <= 0)
                return;

            float audioTimef = audioTime / 1000f;

            for(int i = curKeyframe; i < keyframes.Count; i++)
            {
                var keyframe = keyframes[i];

                if(audioTimef > keyframe.Item1 && audioTimef < keyframe.Item1 + keyframe.Item2)
                {
                    float progress = (audioTimef - keyframe.Item1) / keyframe.Item2;
                    keyframe.Item3.Call(progress);

                    curKeyframe = i;
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            if(luaEnv != null)
            {
                luaEnv.DoString("BanGround:Dispose()");
                luaEnv.Dispose();
            }
            
        }
    }
}
