using System.Reflection;
using UnityEditor;

namespace BanGround.Compoments.Editor
{
    [CustomEditor(typeof(AnimatedIntegerText))]
    public class AnimatedIntegerTextEditor : UnityEditor.UI.TextEditor
    {
        SerializedProperty m_TransitionType;
        SerializedProperty m_TransitionTime;
        SerializedProperty m_Number;
        SerializedProperty m_FontData;
        SerializedProperty m_GoZeroFirst;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_TransitionType = serializedObject.FindProperty("m_TransitionType");
            m_TransitionTime = serializedObject.FindProperty("m_TransitionTime");
            m_Number = serializedObject.FindProperty("next");
            m_FontData = serializedObject.FindProperty("m_FontData");
            m_GoZeroFirst = serializedObject.FindProperty("m_GoZeroFirst");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Number);
            EditorGUILayout.PropertyField(m_TransitionTime);
            EditorGUILayout.PropertyField(m_TransitionType);
            EditorGUILayout.PropertyField(m_GoZeroFirst);
            EditorGUILayout.PropertyField(m_FontData);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}