#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// CutsceneData 인스펙터. CutsceneActionDrawer가 각 액션을 타입별 필드만 표시.
/// </summary>
[CustomEditor(typeof(CutsceneData))]
public class CutsceneDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("actions"), new GUIContent("액션 목록"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
