using UnityEngine;
using System.Collections;

public class FuwafuwaPillar : MonoBehaviour
{
    private SlideNoteBase parent;
    private LineRenderer pillar;
    private const float LINE_WIDTH = 0.2f;

    private void Awake()
    {
        pillar = gameObject.AddComponent<LineRenderer>();
        pillar.enabled = false;
        pillar.useWorldSpace = true;
        pillar.material = Resources.Load<Material>("TestAssets/Materials/note_body");
        pillar.startWidth = LINE_WIDTH * LiveSetting.noteSize;
        pillar.endWidth = LINE_WIDTH * LiveSetting.noteSize;
        pillar.startColor = Color.white;
        pillar.endColor = Color.white;
        pillar.rendererPriority = 1;
    }

    public void Init(SlideNoteBase parent)
    {
        this.parent = parent;
        pillar.enabled = parent.isFuwafuwa;
    }

    void Update()
    {
        if (pillar.enabled && parent != null)
        {
            var pos = parent.transform.position;
            float deltaZ = NoteUtility.GetDeltaZFromJudgePlane(pos);
            pillar.SetPositions(new Vector3[]
            {
                parent.transform.position,
                new Vector3(pos.x, 0, pos.z - deltaZ)
            });
        }
    }
}
