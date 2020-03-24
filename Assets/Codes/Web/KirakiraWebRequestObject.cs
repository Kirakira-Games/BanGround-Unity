using UnityEngine;

public class KirakiraWebRequestObject : MonoBehaviour
{
    public static KirakiraWebRequestObject instance;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
