using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class OpenClosePanel : MonoBehaviour
{
    [Header("패널 설정")]
    [Tooltip("열고 닫을 패널 GameObject")]
    public GameObject targetPanel;

    [Header("동작 모드")]
    [Tooltip("true: 토글 모드 (열려있으면 닫고, 닫혀있으면 열기), false: 항상 열기")]
    public bool toggleMode = false;

    public void OpenPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("OpenClosePanel: targetPanel이 설정되지 않았습니다.", this);
        }
    }

    public void ClosePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("OpenClosePanel: targetPanel이 설정되지 않았습니다.", this);
        }
    }

    // 패널 토글합니다 (열려있으면 닫고, 닫혀있으면 열기)
    public void TogglePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(!targetPanel.activeSelf);
        }
        else
        {
            Debug.LogWarning("OpenClosePanel: targetPanel이 설정되지 않았습니다.", this);
        }
    }

    //Unity 에디터: 플레이모드를 종료합니다
    //빌드된 앱 (PC/모바일): 프로그램/앱을 종료합니다
    public void QuitApplication()
    {
#if UNITY_EDITOR
        // Unity 에디터에서는 플레이모드 종료
        EditorApplication.isPlaying = false;
#else
        // 빌드된 앱에서는 프로그램 종료
        // PC (Windows/Mac/Linux), 모바일 (Android/iOS) 모두 작동
        Application.Quit();
#endif
    }
}

