using System;
using System.Collections.Generic;
using XLua;
using System.IO;
using UnityEngine;
using AudioProvider;
using SELib;
using Zenject;

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
    [Inject]
    private IAudioManager audioManager;

    public ScriptSoundEffect(string file)
    {
        Load(file);
    }

    async void Load(string file)
    {
        var path = ChartScript.GetChartResource(file);

        if (KiraFilesystem.Instance.Exists(path))
        {
            se = await audioManager.PrecacheSE(KiraFilesystem.Instance.Read(path));
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

[LuaCallCSharp]
class ScriptModel : IDisposable
{
    SEModel model;

    GameObject rootObj;

    public ScriptModel(string path)
    {
        model = SEModel.Read(KiraFilesystem.Instance.ReadStream(ChartScript.GetChartResource(path)));

        rootObj = new GameObject(path);

        rootObj.transform.parent = GameObject.Find("ScriptObjects").transform;

        var bones = new List<Transform>();

        foreach (var bone in model.Bones)
        {
            var boneObj = new GameObject(bone.BoneName);

            if (bone.RootBone)
                boneObj.transform.parent = rootObj.transform;
            else
                boneObj.transform.parent = bones[bone.BoneParent];

            boneObj.transform.position = new Vector3((float)bone.LocalPosition.X, (float)bone.LocalPosition.Y, (float)bone.LocalPosition.Z);
            boneObj.transform.rotation = new Quaternion((float)bone.LocalRotation.X, (float)bone.LocalRotation.Y, (float)bone.LocalRotation.Z, (float)bone.LocalRotation.W);

            bones.Add(boneObj.transform);
        }

        foreach (var mesh in model.Meshes)
        {
            var modelObj = new GameObject(path);

            modelObj.transform.parent = rootObj.transform;

            var meshRenderer = modelObj.AddComponent<SkinnedMeshRenderer>();

            var umesh = new Mesh();

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var UVs = new List<Vector2>();
            var boneWeights = new List<BoneWeight>();
            var triangles = new List<int>();
            var bindPoses = new List<Matrix4x4>();

            int baseIndex = verts.Count;

            foreach (var vert in mesh.Verticies)
            {
                verts.Add(new Vector3((float)vert.Position.X, (float)vert.Position.Y, (float)vert.Position.Z));
                normals.Add(new Vector3((float)vert.VertexNormal.X, (float)vert.VertexNormal.Y, (float)vert.VertexNormal.Z));
                UVs.Add(new Vector2((float)vert.UVSets[0].X, (float)vert.UVSets[0].Y));

                bindPoses.Add(Matrix4x4.identity);

                var weight = new BoneWeight();

                for (int i = 0; i < vert.WeightCount; i++)
                {
                    if (i == 0)
                    {
                        weight.boneIndex0 = (int)vert.Weights[i].BoneIndex;
                        weight.weight0 = vert.Weights[i].BoneWeight;
                    }
                    else if (i == 1)
                    {
                        weight.boneIndex1 = (int)vert.Weights[i].BoneIndex;
                        weight.weight1 = vert.Weights[i].BoneWeight;
                    }
                    else if (i == 2)
                    {
                        weight.boneIndex2 = (int)vert.Weights[i].BoneIndex;
                        weight.weight2 = vert.Weights[i].BoneWeight;
                    }
                    else if (i == 3)
                    {
                        weight.boneIndex3 = (int)vert.Weights[i].BoneIndex;
                        weight.weight3 = vert.Weights[i].BoneWeight;
                    }
                }

                boneWeights.Add(weight);
            }

            foreach (var face in mesh.Faces)
            {
                triangles.Add(baseIndex + (int)face.FaceIndex1);
                triangles.Add(baseIndex + (int)face.FaceIndex2);
                triangles.Add(baseIndex + (int)face.FaceIndex3);
            }

            umesh.vertices = verts.ToArray();
            umesh.normals = normals.ToArray();
            umesh.uv = UVs.ToArray();
            umesh.triangles = triangles.ToArray();
            umesh.boneWeights = boneWeights.ToArray();
            umesh.bindposes = bindPoses.ToArray();

            umesh.RecalculateBounds();
            umesh.Optimize();

            meshRenderer.sharedMesh = umesh;
            meshRenderer.quality = SkinQuality.Bone2;
            meshRenderer.bones = bones.ToArray();
            meshRenderer.rootBone = bones[0];
        }
    }

    public void Dispose()
    {
    }
}

[LuaCallCSharp]
class ScriptTexture
{
    public Texture2D tex;

    public ScriptTexture(string file)
    {
        var path = ChartScript.GetChartResource(file);
        tex = KiraFilesystem.Instance.ReadTexture2D(path);
    }
}

[LuaCallCSharp]
class ScriptEnvironment
{
    public void SetBackground(ScriptTexture tex)
    {
        InGameBackground.instance.SetBackground(tex.tex);
    }
    public void SetLaneBackground(ScriptTexture tex)
    {
        var mesh = GameObject.Find("LaneBackground").GetComponent<MeshRenderer>();
        var material = UnityEngine.Object.Instantiate(mesh.sharedMaterial);
        material.SetTexture("_BaseMap", tex.tex);

        mesh.sharedMaterial = material;
    }
    public void SetJudgeLineColor(float r, float g, float b)
    {
        var mesh = GameObject.Find("JudgeLine").GetComponent<MeshRenderer>();
        var material = UnityEngine.Object.Instantiate(mesh.sharedMaterial);
        material.SetColor("_BaseColor", new Color(r, g, b));

        mesh.sharedMaterial = material;
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

            luaEnv.AddLoader((ref string lib) =>
            {
                var modulepath = GetChartResource(lib + ".lua");

                if (KiraFilesystem.Instance.Exists(modulepath))
                {
                    return KiraFilesystem.Instance.Read(modulepath);
                }

                return null;
            });

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