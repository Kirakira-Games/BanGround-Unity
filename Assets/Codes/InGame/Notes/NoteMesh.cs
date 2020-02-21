using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteMesh
{
    static readonly Vector2[] uv =
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };

    static readonly Vector3[] normals =
    {
        Vector3.up,
        Vector3.up,
        Vector3.up,
        Vector3.up
    };

    static readonly int[] indices = { 0, 2, 1, 2, 3, 1 };
    
    const float deltaPerLane = 0.07f;

    static private Mesh[] noteMesh;
    static private int sortingLayerID;
    static private Material mat;

    private static Mesh CreateMesh(int lane)
    {
        float delta = (lane - 3) * deltaPerLane;
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-1 + delta, -0.4f),
            new Vector3(1 + delta, -0.4f),
            new Vector3(-1 - delta, 0.4f),
            new Vector3(1 - delta, 0.4f)
        };
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            normals = normals,
            triangles = indices
        };
        mesh.RecalculateBounds();
        return mesh;
    }

    public static void Init()
    {
        noteMesh = new Mesh[NoteUtility.LANE_COUNT];
        for (int i = 0; i < noteMesh.Length; i++)
        {
            noteMesh[i] = CreateMesh(i);
        }
        sortingLayerID = SortingLayer.NameToID("Note");
        mat = Resources.Load<Material>("TestAssets/Materials/note");
    }

    public static MeshRenderer Create(GameObject note, int lane)
    {
        MeshRenderer meshRenderer = note.GetComponent<MeshRenderer>();
        meshRenderer.material = mat;

        MeshFilter meshFilter = note.GetComponent<MeshFilter>();
        meshFilter.mesh = noteMesh[lane];

        meshRenderer.sortingLayerID = sortingLayerID;
        return meshRenderer;
    }
}
