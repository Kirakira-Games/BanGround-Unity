using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
 
public class ChangeFontWindow : EditorWindow
{
    [MenuItem("BanGround/更换字体")]
    public static void Open()
    {
        EditorWindow.GetWindow(typeof(ChangeFontWindow));
    }
 
    Font toChange;
    static Font toChangeFont;
    FontStyle toFontStyle;
    static FontStyle toChangeFontStyle;
 
    void OnGUI()
    {
        toChange = (Font)EditorGUILayout.ObjectField(toChange, typeof(Font), true, GUILayout.MinWidth(100f));
        toChangeFont = toChange;
        toFontStyle = (FontStyle)EditorGUILayout.EnumPopup(toFontStyle, GUILayout.MinWidth(100f));
        toChangeFontStyle = toFontStyle;
        if (GUILayout.Button("更换"))
        {
            Change();
        }
    }
 
    public static void Change()
    {
        //寻找Hierarchy面板下所有的Text
        var tArray = Resources.FindObjectsOfTypeAll(typeof(Text));
        for (int i = 0; i < tArray.Length; i++)
        {
            Text t = tArray[i] as Text;
            //这个很重要，博主发现如果没有这个代码，unity是不会察觉到编辑器有改动的，自然设置完后直接切换场景改变是不被保存
            //的  如果不加这个代码  在做完更改后 自己随便手动修改下场景里物体的状态 在保存就好了 
            Undo.RecordObject(t, t.gameObject.name);
            t.font = toChangeFont;
            t.fontStyle = toChangeFontStyle;
            //相当于让他刷新下 不然unity显示界面还不知道自己的东西被换掉了  还会呆呆的显示之前的东西
            EditorUtility.SetDirty(t);
        }
        Debug.Log("Succed");
    }
}
