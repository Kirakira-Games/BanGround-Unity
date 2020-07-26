using FMOD;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BGEditor
{
    public class EditorSlideBody : Image
    {
        private static readonly Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };
        private static readonly int[] triangles = new int[]
        {
            1, 0, 3, 3, 0, 2
        };
        private static Material[] materials;
        public Vector3 widthDelta;
        public EditorSlideNote parent;

        public PolygonCollider2D polyCollider;
        private Vector3[] vertices;

        protected override void Awake()
        {
            base.Awake();
            if (materials == null)
            {
                materials = new Material[]
                {
                    new Material(Resources.Load<Material>("InGame/Materials/note_body")),
                    new Material(Resources.Load<Material>("InGame/Materials/note_body")),
                    new Material(Resources.Load<Material>("InGame/Materials/note_body"))
                };
                materials[0].SetColor("_BaseColor", new Color(0.5843137f, 0.9019607f, 0.3019607f, 0.8f));
                materials[1].SetColor("_BaseColor", new Color(0.29f, 0.9019607f, 0.3019607f, 0.8f));
                materials[2].SetColor("_BaseColor", new Color(0.5843137f, 0.9019607f, 0.3019607f, 0.3f));
            }
            material = materials[0];
            widthDelta = GetComponent<RectTransform>().rect.width * Vector3.right / 2;
            vertices = new Vector3[uvs.Length];
            polyCollider = GetComponent<PolygonCollider2D>();
            parent = GetComponentInParent<EditorSlideNote>();
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (vertices == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }
            toFill.Clear();
            for (int i = 0; i < vertices.Length; i++)
                toFill.AddVert(vertices[i], Color.white, uvs[i]);
            for (int i = 0; i < triangles.Length; i += 3)
                toFill.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
        }

        public void SetDirection(Vector3 dir)
        {
            vertices[0] = -widthDelta;
            vertices[1] = widthDelta;
            vertices[2] = dir - widthDelta;
            vertices[3] = dir + widthDelta;
            SetVerticesDirty();
            var points = vertices.Select(p => new Vector2(p.x, p.y)).ToList();
            (points[2], points[3]) = (points[3], points[2]);
            points.Add(points[0]);
            polyCollider.points = points.ToArray();
        }

        public void SetColor(int index)
        {
            material = materials[index];
        }
    }
}
