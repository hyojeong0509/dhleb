using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ESC 키로 열리는 게임 내 메뉴
/// 1. 돌아가기 2. 설정 3. 타이틀로 돌아가기 4. 나가기
/// </summary>
public class GamePauseMenu : MonoBehaviour
{
    [Header("메뉴 패널")]
    public GameObject menuPanel;

    [Header("설정 패널")]
    public GameObject settingsPanel;

    [Header("버튼")]
    public Button btnResume;        // 1. 돌아가기  (그냥 다시 게임하기)
    public Button btnSettings;     // 2. 설정
    public Button btnBackToTitle;   // 3. 타이틀로 돌아가기
    public Button btnExit;         // 4. 나가기 (게임아예 끄기)

    private bool isOpen;

    void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (btnResume != null) btnResume.onClick.AddListener(CloseMenu);
        if (btnSettings != null) btnSettings.onClick.AddListener(OnClickSettings);
        if (btnBackToTitle != null) btnBackToTitle.onClick.AddListener(OnClickBackToTitle);
        if (btnExit != null) btnExit.onClick.AddListener(OnClickExit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (isOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    void OpenMenu()
    {
        isOpen = true;
        if (menuPanel != null) menuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        GameInputBlocker.Block();
        Time.timeScale = 0f; // 게임 전체 일시정지 (이동, 애니메이션, 시간 등)
        if (TimeManager.Instance != null)
            TimeManager.Instance.SetPause(true);
    }

    void CloseMenu()
    {
        isOpen = false;
        if (menuPanel != null) menuPanel.SetActive(false);

        GameInputBlocker.Unblock();
        Time.timeScale = 1f;
        if (TimeManager.Instance != null)
            TimeManager.Instance.SetPause(false);
    }

    void OnClickSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void OnClickBackToTitle()
    {
        CloseMenu();

        if (GameDataManager.Instance != null && GameDataManager.Instance.currentSaveData != null)
            GameDataManager.Instance.SaveGame();

        GameDataManager.CleanupGameManagersBeforeTitle();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    void OnClickExit()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.currentSaveData != null)
            GameDataManager.Instance.SaveGame();

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
