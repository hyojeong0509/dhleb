using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// 편집 시 툴팁/커서가 화면을 가릴 때, 비활성화 대신 alpha로 숨김.
// 비활성화하면 스크립트가 안 돌아서 툴팁/커서가 안 나오므로,
// alpha=0으로만 숨기면 오브젝트는 활성 상태라 플레이 시 정상 동작.
public static class TooltipCursorVisibility
{
    const string PrefKey = "TooltipCursorHiddenForEditing";

    [MenuItem("Tools/UI/툴팁 & 커서 숨기기 (편집용)")]
    static void HideForEditing()
    {
        SetVisibility(false);
        EditorPrefs.SetBool(PrefKey, true);
    }

    [MenuItem("Tools/UI/툴팁 & 커서 보이기")]
    static void Show()
    {
        SetVisibility(true);
        EditorPrefs.SetBool(PrefKey, false);
    }

    [MenuItem("Tools/UI/툴팁 & 커서 표시 전환")]
    static void Toggle()
    {
        bool hidden = EditorPrefs.GetBool(PrefKey, false);
        bool show = hidden;
        SetVisibility(show);
        EditorPrefs.SetBool(PrefKey, !show);
    }

    static void SetVisibility(bool visible)
    {
        float alpha = visible ? 1f : 0f;
        int count = 0;

        foreach (var tooltip in Object.FindObjectsOfType<ItemTooltip>(true))
        {
            var cg = tooltip.GetComponent<CanvasGroup>();
            if (cg == null) cg = tooltip.gameObject.AddComponent<CanvasGroup>();
            Undo.RecordObject(cg, "Tooltip/Cursor Visibility");
            cg.alpha = alpha;
            count++;
        }

        foreach (var cursor in Object.FindObjectsOfType<CursorItem>(true))
        {
            var cg = cursor.GetComponent<CanvasGroup>();
            if (cg == null) cg = cursor.gameObject.AddComponent<CanvasGroup>();
            Undo.RecordObject(cg, "Tooltip/Cursor Visibility");
            cg.alpha = alpha;
            count++;
        }

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[TooltipCursorVisibility] {(visible ? "보이기" : "숨기기")} 적용 ({count}개)");
        }
    }

    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // 편집용으로 숨겨둔 상태였으면, 플레이 시에는 보이게
            if (EditorPrefs.GetBool(PrefKey, false))
            {
                EditorApplication.delayCall += () =>
                {
                    if (Application.isPlaying)
                        SetVisibilityRuntime(true);
                };
            }
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // 플레이 종료 후, 편집용 숨김 상태로 복원
            if (EditorPrefs.GetBool(PrefKey, false))
            {
                EditorApplication.delayCall += () => SetVisibility(false);
            }
        }
    }

    static void SetVisibilityRuntime(bool visible)
    {
        float alpha = visible ? 1f : 0f;
        foreach (var tooltip in Object.FindObjectsOfType<ItemTooltip>(true))
        {
            var cg = tooltip.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = alpha;
        }
        foreach (var cursor in Object.FindObjectsOfType<CursorItem>(true))
        {
            var cg = cursor.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = alpha;
        }
    }
}
