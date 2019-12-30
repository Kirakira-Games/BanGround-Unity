using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 绿条
public class NoteMesh : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3[] meshVertices;

    public Material material;
    public Transform frontNoteTrans;
    public Transform afterNoteTrans;

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

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = indices;
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshRenderer.material = material;
    }
}
