using UnityEngine;
using UnityEngine.UI;

public class FloatInput : MonoBehaviour
{
    public float Default;
    public float MinVal;
    public float MaxVal;
    public float value { get; private set; }

    public Color NormalColor;
    public Color ErrorColor;

    private InputField Component;

    private void Validate(string _)
    {
        if (!float.TryParse(Component.text, out float val) || val < MinVal || val > MaxVal)
        {
            Component.textComponent.color = ErrorColor;
        }
        else
        {
            value = val;
            Component.textComponent.color = NormalColor;
        }
    }

    public void SetValue(float value)
    {
        if (Component == null)
        {
            Init();
        }
        Component.text = value.ToString();
        this.value = value;
    }

    private void Init()
    {
        Component = GetComponent<InputField>();
        Component.onValueChanged.AddListener(Validate);
        Component.text = Default.ToString();
    }

    private void Awake()
    {
        if (Component == null)
        {
            Init();
        }
    }
}
