using System;
using UnityEngine;
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
        public Vector3 widthDelta;
        public Vector3 direction;

        protected override void Awake()
        {
            base.Awake();
            widthDelta = GetComponent<RectTransform>().rect.width * Vector3.right / 2;
            material = new Material(material);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
            toFill.AddVert(-widthDelta, Color.white, uvs[0]);
            toFill.AddVert(widthDelta, Color.white, uvs[1]);
            toFill.AddVert(direction - widthDelta, Color.white, uvs[2]);
            toFill.AddVert(direction + widthDelta, Color.white, uvs[3]);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                toFill.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            }
        }

        public void SetDirection(Vector2 dir)
        {
            direction = dir;
            SetVerticesDirty();
        }

        public void SetColor(Color color)
        {
            material.SetColor("_BaseColor", color);
        }
    }
}
