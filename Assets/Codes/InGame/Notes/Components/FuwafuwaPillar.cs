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
        pillar.startWidth = LINE_WIDTH * r_notesize;
        pillar.endWidth = LINE_WIDTH * r_notesize;
        pillar.rendererPriority = 1;
        gameObject.layer = 8; // note
    }

    public void Reset(SlideNoteBase parent)
    {
        this.parent = parent;
        pillar.enabled = parent.displayFuwafuwa;
        pillar.sharedMaterial = parent.noteMesh.meshRenderer.sharedMaterial;
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
