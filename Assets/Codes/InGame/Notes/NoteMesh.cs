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

    public static MeshRenderer Create(GameObject note, int lane)
    {
        MeshRenderer meshRenderer = note.AddComponent<MeshRenderer>();
        Material material = Resources.Load<Material>("TestAssets/Materials/note");
        meshRenderer.material = material;

        MeshFilter meshFilter = note.AddComponent<MeshFilter>();
        float delta = (lane - 3) * 0.1f;
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-1 + delta, -0.5f),
            new Vector3(1 + delta, -0.5f),
            new Vector3(-1 - delta, 0.5f),
            new Vector3(1 - delta, 0.5f)
        };
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            normals = normals,
            triangles = indices
        };
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshRenderer.sortingLayerID = SortingLayer.NameToID("Note");
        return meshRenderer;
    }
}
