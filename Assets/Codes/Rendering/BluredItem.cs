using UnityEngine;

public class BluredItem : MonoBehaviour
{
    private void Awake()
    {
        if (!DualKawaseBlur.BluredItems.Contains(this))
            DualKawaseBlur.BluredItems.Add(this);
    }
        
    private void OnEnable()
    {
        if (!DualKawaseBlur.BluredItems.Contains(this))
            DualKawaseBlur.BluredItems.Add(this);
    }

    private void OnDisable()
    {
        if (DualKawaseBlur.BluredItems.Contains(this))
            DualKawaseBlur.BluredItems.Remove(this);
    }

    private void OnDestroy()
    {
        if (DualKawaseBlur.BluredItems.Contains(this))
            DualKawaseBlur.BluredItems.Remove(this);
    }
}
