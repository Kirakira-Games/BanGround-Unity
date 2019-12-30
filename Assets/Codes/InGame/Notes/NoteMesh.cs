using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 绿条
public class NoteMesh : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3[] meshVertices;

    public Transform afterNoteTrans;
    public const float BODY_WIDTH = 0.6f;

    public static NoteMesh Create(Transform start, Transform after)
    {
        GameObject slideMesh = new GameObject("SlideBody");
        NoteMesh mesh = slideMesh.AddComponent<NoteMesh>();
        mesh.transform.SetParent(start);
        mesh.transform.localPosition = new Vector3(0f, -0.01f);
        mesh.afterNoteTrans = after;
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

    public void Start()
    {
        InitMesh();
        OnUpdate();
    }

    public void OnUpdate()
    {
        Vector3 delta = afterNoteTrans.position - transform.parent.position;
        Vector3[] vertices =
        {
            new Vector3(-BODY_WIDTH, 0, 0),
            new Vector3(BODY_WIDTH, 0, 0),
            new Vector3(delta.x - BODY_WIDTH, 0, delta.z),
            new Vector3(delta.x + BODY_WIDTH, 0, delta.z)
        };
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x /= transform.parent.localScale.x;
            vertices[i].y /= transform.parent.localScale.y;
            vertices[i].z /= transform.parent.localScale.z;
        }
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
        material.mainTexture = Resources.Load<Texture2D>("V2Assets/long_note_line");
        meshRenderer.material = material;
    }
}
