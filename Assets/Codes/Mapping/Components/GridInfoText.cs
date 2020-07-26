using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GridInfoText : MonoBehaviour
{
    public Text text;
    public Image image;

    void Awake()
    {
        text = gameObject.AddComponent<Text>();
        image = gameObject.AddComponent<Image>();
    }

    public void Reset()
    {

    }
}
