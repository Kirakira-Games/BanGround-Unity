using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 绿条
public class SlideMesh : MonoBehaviour
{
    public const float BODY_WIDTH = 0.8f;

    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public Transform transS;
    public Transform transT;

    public static Material cacheMat = null;
    public static readonly Vector3 initPos = new Vector3(0, -0.01f);

    public void ResetMesh(Transform S, Transform T, bool isFuwafuwa)
    {
        transform.SetParent(S);
        transform.localPosition = initPos;
        transS = S;
        transT = T;
        SetFuwafuwa(isFuwafuwa);
    }

    readonly Vector2[] uv =
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };

    public void SetFuwafuwa(bool isFuwafuwa)
    {
        gameObject.layer = isFuwafuwa ? 9 : 8;
    }

    public void OnUpdate()
    {
        Vector3 delta = transT.position - transform.parent.position;
        float width = BODY_WIDTH * LiveSetting.noteSize;
        Vector3[] vertices =
        {
            new Vector3(-width, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(delta.x - width, delta.y, delta.z),
            new Vector3(delta.x + width, delta.y, delta.z)
        };
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();
    }

    public void InitMesh()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        var meshVertices = new Vector3[]
        {
           Vector3.zero,
           Vector3.zero,
           Vector3.zero,
           Vector3.zero
        };

        int[] indices = { 0, 2, 1, 2, 3, 1 };

        Mesh mesh = new Mesh
        {
            vertices = meshVertices,
            uv = uv,
            triangles = indices
        };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        if (cacheMat == null)
        {
            Material material = Resources.Load<Material>("InGame/Materials/note_body");
            cacheMat = Instantiate(material);
            cacheMat.mainTexture = NoteUtility.LoadResource<Texture2D>("long_note_mask");
            cacheMat.SetColor("_BaseColor", new Color(0.5843137f, 0.9019607f, 0.3019607f, LiveSetting.longBrightness));
        }
        meshRenderer.material = cacheMat;
    }
}
