using UnityEngine;
using UnityEngine.UI;

public class FloatInput : MonoBehaviour
{
    public float Default;
    public float MinVal;
    public float MaxVal;
    public float value { get; private set; }

    private InputField Component;

    private void Validate(string _)
    {
        if (!float.TryParse(Component.text, out float val) || val < MinVal || val > MaxVal)
        {
            Component.textComponent.color = Color.red;
        }
        else
        {
            value = val;
            Component.textComponent.color = Color.black;
        }
    }

    public void SetValue(float value)
    {
        Component.text = value.ToString();
    }

    void Awake()
    {
        Component = GetComponent<InputField>();
        Component.onValueChanged.AddListener(Validate);
        Component.text = Default.ToString();
    }
}
