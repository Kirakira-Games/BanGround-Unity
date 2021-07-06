using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Assets.Editor {
    class FindAllRaycastTargets : EditorWindow {
        private Vector2 scrollPos = Vector2.zero;

        [MenuItem("BanGround/Find all raycast targets")]
        static void OpenWindow() {
            var window = (FindAllRaycastTargets)GetWindow(typeof(FindAllRaycastTargets), false, "Raycast Targets");
            window.RefreshTargets();
            window.Show();
        }

        private List<Graphic> raycastTargets = new List<Graphic>();

        private void RefreshTargets() {
            raycastTargets = (
                from graphic in (
                    from obj in EditorUtil.AllGameObjectsInScene(SceneManager.GetActiveScene())
                    select obj.GetComponent<Graphic>()
                )
                where graphic != null && graphic.raycastTarget
                select graphic
            ).ToList();
        }

        void OnGUI() {
            // Title
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh")) {
                RefreshTargets();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Table
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var obj in raycastTargets) {
                EditorGUILayout.ObjectField(obj, obj.GetType(), false);
            }
            GUILayout.EndScrollView();
        }
    }
}
