using UnityEngine;
using System.Collections;

public class FuwafuwaPillar : MonoBehaviour
{
    private SlideNoteBase parent;
    private LineRenderer pillar;
    private const float LINE_WIDTH = 0.2f;

    static KVarRef r_notesize = new KVarRef("r_notesize");

    private void Awake()
    {
        pillar = gameObject.AddComponent<LineRenderer>();
        pillar.enabled = false;
        pillar.useWorldSpace = true;
        pillar.receiveShadows = false;
        pillar.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        pillar.material = Resources.Load<Material>("InGame/Materials/note_body");
        pillar.startWidth = LINE_WIDTH * r_notesize;
        pillar.endWidth = LINE_WIDTH * r_notesize;
        pillar.startColor = Color.white;
        pillar.endColor = Color.white;
        pillar.rendererPriority = 1;
        gameObject.layer = 8; // note
    }

    public void Init(SlideNoteBase parent)
    {
        this.parent = parent;
        pillar.enabled = parent.displayFuwafuwa;
    }

    public void OnUpdate()
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
