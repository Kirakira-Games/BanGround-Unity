using UnityEngine;

public class FuwafuwaPillar : MonoBehaviour
{
    private SlideNoteBase parent;
    private LineRenderer pillar;
    private const float LINE_WIDTH = 0.3f;

    public void Inject(KVar r_notesize)
    {
        pillar.startWidth = LINE_WIDTH * r_notesize;
        pillar.endWidth = LINE_WIDTH * r_notesize;
    }

    private void Awake()
    {
        pillar = gameObject.AddComponent<LineRenderer>();
        pillar.enabled = false;
        pillar.useWorldSpace = true;
        pillar.receiveShadows = false;
        pillar.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        pillar.rendererPriority = 1;
        gameObject.layer = 8; // note
    }

    public void Reset(SlideNoteBase parent, Material material)
    {
        this.parent = parent;
        pillar.enabled = parent.displayFuwafuwa;
        pillar.sharedMaterial = material;
    }

    public void OnUpdate()
    {
        if (pillar.enabled && parent != null)
        {
            var pos = parent.transform.position;
            float deltaZ = NoteUtility.GetDeltaZFromJudgePlane(pos.y);
            pillar.SetPositions(new Vector3[]
            {
                parent.transform.position,
                new Vector3(pos.x, 0, pos.z - deltaZ)
            });
        }
    }
}
