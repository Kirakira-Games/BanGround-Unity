using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Editor {
    internal static class EditorUtil {
        public static IEnumerable<GameObject> AllGameObjectsInScene(Scene scene) {
            foreach (var obj in scene.GetRootGameObjects()) {
                foreach (var childObj in obj.GetComponentsInChildren<Transform>(true)) {
                    yield return childObj.gameObject;
                }
            }
        }
    }
}