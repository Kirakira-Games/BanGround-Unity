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

    static private int sortingLayerID;
    static private Material mat;

    public static void CreateMesh(GameObject note)
    {
        MeshFilter meshFilter = note.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = note.AddComponent<MeshRenderer>();

        meshRenderer.material = mat;
        Mesh mesh = new Mesh
        {
            vertices = GetVertices(3),
            uv = uv,
            normals = normals,
            triangles = indices
        };
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshRenderer.sortingLayerID = sortingLayerID;
    }

    private static Vector3[] GetVertices(int lane)
    {
        float delta = (lane - 3) * deltaPerLane;
        return new Vector3[]
        {
            new Vector3(-1 + delta, -0.4f),
            new Vector3(1 + delta, -0.4f),
            new Vector3(-1 - delta, 0.4f),
            new Vector3(1 - delta, 0.4f)
        };
    }

    public static void Init()
    {
        sortingLayerID = SortingLayer.NameToID("Note");
        mat = Resources.Load<Material>("TestAssets/Materials/note");
    }

    public static void Reset(GameObject note, int lane)
    {
        note.GetComponent<MeshFilter>().mesh.SetVertices(GetVertices(lane));
    }
}
