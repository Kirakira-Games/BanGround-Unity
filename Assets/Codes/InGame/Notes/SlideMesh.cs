using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 绿条
public class SlideMesh : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3[] meshVertices;

    public Transform afterNoteTrans;
    public const float BODY_WIDTH = 0.7f;

    public static SlideMesh Create(Transform start, Transform after)
    {
        GameObject slideMesh = new GameObject("SlideBody");
        SlideMesh mesh = slideMesh.AddComponent<SlideMesh>();
        mesh.transform.SetParent(start);
        mesh.transform.localPosition = new Vector3(0f, -0.01f);
        mesh.afterNoteTrans = after;
        mesh.InitMesh();
        slideMesh.AddComponent<SortingGroup>().sortingLayerID = SortingLayer.NameToID("SlideBody");
        return mesh;
    }

    readonly Vector2[] uv =
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };

    readonly Vector3[] normals =
    {
        Vector3.up,
        Vector3.up,
        Vector3.up,
        Vector3.up
    };

    public void OnUpdate()
    {
        Vector3 delta = afterNoteTrans.position - transform.parent.position;
        float width = BODY_WIDTH * LiveSetting.noteSize;
        Vector3[] vertices =
        {
            new Vector3(-width, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(delta.x - width, 0, delta.z),
            new Vector3(delta.x + width, 0, delta.z)
        };
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateBounds();
    }

    public void InitMesh()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshVertices = new Vector3[]
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
            normals = normals,
            uv = uv,
            triangles = indices
        };
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        Material material = Resources.Load<Material>("TestAssets/Materials/note_body");
        material.mainTexture = NoteUtility.LoadResource<Texture2D>("long_note_mask");
        meshRenderer.material = material;
    }
}
