using UnityEngine;
using UnityEngine.UI;

public class GetVersion : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = Application.version;
    }
}
