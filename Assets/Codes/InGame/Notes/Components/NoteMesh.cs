using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class NoteSprite
{
    static private int sortingLayerID;
    static private Material mat;

    public static void CreateMesh(GameObject note)
    {
        var spriteRenderer = note.AddComponent<SpriteRenderer>();
        spriteRenderer.material = mat;
        spriteRenderer.sortingLayerID = sortingLayerID;
    }

    public static void Init()
    {
        sortingLayerID = SortingLayer.NameToID("Note");
        mat = Resources.Load<Material>("InGame/Materials/note");
    }
}
*/

public class NoteMesh : MonoBehaviour
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

    static private int sortingLayerID;
    static private Material mat;
    static private float screenYStart;
    static private float screenYEnd;
    static private float[] cachedLength;

    public MeshFilter meshFilter { get; private set; }
    public MeshRenderer meshRenderer { get; private set; }
    public float width = 1.1f;
    private Vector3[] meshVertices;

    private static float GetLength(float z)
    {
        Vector3 screenPos = NoteController.mainCamera.WorldToScreenPoint(new Vector3(0, 0, z));
        float ratio = Mathf.InverseLerp(screenYStart, screenYEnd, screenPos.y);
        ratio = Mathf.Lerp(0.05f, 1f, ratio) * Mathf.Lerp(0.8f, 1f, ratio);
        screenPos.y = Mathf.Max(0, screenPos.y - ratio * (screenYStart - screenYEnd) / 20);
        Ray ray = NoteController.mainCamera.ScreenPointToRay(screenPos);
        float delta = -ray.origin.y / ray.direction.y;
        Vector3 p = ray.GetPoint(delta);
        return z - p.z;
    }

    private static float GetCachedLength(float z)
    {
        float ratio = Mathf.InverseLerp(NoteUtility.NOTE_START_Z_POS, NoteUtility.NOTE_JUDGE_Z_POS, z);
        if (ratio >= 1 - NoteUtility.EPS) return cachedLength[cachedLength.Length - 1];
        ratio = Mathf.Max(0, ratio) * (cachedLength.Length - 1);
        int index = (int)ratio;
        ratio -= index;
        return Mathf.Lerp(cachedLength[index], cachedLength[index+1], ratio);
    }

    private Vector3[] GetVertices(float z)
    {
        float dz = GetCachedLength(z);
        meshVertices[0].z = -dz;
        meshVertices[1].z = -dz;
        meshVertices[2].z = dz;
        meshVertices[3].z = dz;
        return meshVertices;
    }

    public static void Init()
    {
        sortingLayerID = SortingLayer.NameToID("Note");
        mat = Resources.Load<Material>("InGame/Materials/note");

        screenYStart = NoteController.mainCamera.WorldToScreenPoint(
            new Vector3(0, 0, NoteUtility.NOTE_START_Z_POS)).y;
        screenYEnd = NoteController.mainCamera.WorldToScreenPoint(
            new Vector3(0, 0, NoteUtility.NOTE_JUDGE_Z_POS)).y;

        cachedLength = new float[64];
        for (int i = 0; i < cachedLength.Length; i++)
        {
            float z = Mathf.Lerp(NoteUtility.NOTE_START_Z_POS, NoteUtility.NOTE_JUDGE_Z_POS, (float)i / (cachedLength.Length - 1));
            cachedLength[i] = GetLength(z);
        }
    }

    private void Awake()
    {
        meshVertices = new Vector3[]
        {
            new Vector3(-width, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(-width, 0.001f, 0),
            new Vector3(width, 0.001f, 0)
        };

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = mat;
        Mesh mesh = new Mesh
        {
            vertices = GetVertices(NoteUtility.GetInitPos(3).z),
            uv = uv,
            normals = normals,
            triangles = indices
        };
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshRenderer.sortingLayerID = sortingLayerID;
    }

    public void OnUpdate()
    {
        meshFilter.mesh.SetVertices(GetVertices(transform.position.z));
        meshFilter.mesh.RecalculateBounds();
    }
}
