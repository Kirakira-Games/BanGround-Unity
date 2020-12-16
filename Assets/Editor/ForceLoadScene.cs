using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Editor
{
    public class ForceLoadScene : ScriptableObject
    {
        [MenuItem("BanGround/Force load Select scene")]
        static void ForceLoadSelectScene()
        {
            ForceLoadSceneByName("Select");
        }

        static void ForceLoadSceneByName(string name)
        {
            SceneManager.LoadScene(name);
        }
    }
}