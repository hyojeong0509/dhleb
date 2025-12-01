using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("로딩 텍스트 설정")]
    [SerializeField] private string[] loadingMessages = {
        "우주를 탐험하는 중...",
        "행성을 스캔하는 중...",
        "데이터를 로드하는 중...",
        "준비 중..."
    };

    [Header("애니메이션 설정")]
    [SerializeField] private float textChangeInterval = 0.8f; // 텍스트 변경 간격 (초)

    private float timer = 0f;
    private int currentMessageIndex = 0;

    void Start()
    {
        // UI 초기화
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }

        if (loadingText != null && loadingMessages.Length > 0)
        {
            loadingText.text = loadingMessages[0];
        }
    }

    void Update()
    {
        // 로딩 바 업데이트
        if (loadingBar != null && GameSceneManager.Instance != null)
        {
            float progress = GameSceneManager.Instance.GetLoadingProgress();
            loadingBar.value = progress;
        }

        // 로딩 메시지 변경
        timer += Time.deltaTime;
        if (timer >= textChangeInterval && loadingMessages.Length > 0)
        {
            timer = 0f;
            currentMessageIndex = (currentMessageIndex + 1) % loadingMessages.Length;
            
            if (loadingText != null)
            {
                loadingText.text = loadingMessages[currentMessageIndex];
            }
        }
    }
}

