using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Editor {
    internal static class EditorUtil {
        private static IEnumerable<GameObject> DfsGameObjects(GameObject obj) {
            yield return obj;
            foreach (var child in obj.GetComponentsInChildren<Transform>(true)) {
                if (child.gameObject == obj) {
                    continue;
                }
                foreach (var childObj in DfsGameObjects(child.gameObject)) {
                    yield return childObj;
                }
            }
        }
        public static IEnumerable<GameObject> AllGameObjectsInScene(Scene scene) {
            foreach (var obj in scene.GetRootGameObjects()) {
                foreach (var ret in DfsGameObjects(obj)) {
                    yield return ret;
                }
            }
        }
    }
}