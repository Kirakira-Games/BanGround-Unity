using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Editor
{
    class ListLocalizedTexts : EditorWindow
    {
        [MenuItem("BanGround/List all localized texts")]
        static void OpenWindow()
        {
            var window = (ListLocalizedTexts)GetWindow(typeof(ListLocalizedTexts), false, "Localized Texts");
            window.RefreshTargets();
            window.Show();
        }

        private List<string> texts = new List<string>();
        private string language = "english";

        private void RefreshTargets()
        {
            var lang = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/LanguageFiles/{language}.txt")?.text;

            if (lang == null)
                return;

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(lang);

            texts = (
                from text in (
                    from obj in EditorUtil.AllGameObjectsInScene(SceneManager.GetActiveScene())
                    select obj.GetComponent<LocalizedText>()
                )
                where text != null && text.text != null && !dict.ContainsKey(text.text)
                select text.text
            ).ToList();
        }

        private Vector2 scrollPos = Vector2.zero;

        void OnGUI()
        {
            // Title
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Uncovered Total: {texts.Count}");
            GUILayout.Label("Target Language:");
            language = GUILayout.TextField(language);

            if (GUILayout.Button("Refresh"))
            {
                RefreshTargets();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // Table
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var obj in texts)
            {
                EditorGUILayout.TextField(obj);
            }
            GUILayout.EndScrollView();
        }
    }
}
